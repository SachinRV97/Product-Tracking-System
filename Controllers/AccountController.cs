using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Services;
using ProductTrackingSystem.ViewModels;

namespace ProductTrackingSystem.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;

    public AccountController(ApplicationDbContext context, IPasswordHasher passwordHasher, IAuditService audit)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        ViewBag.Companies = await _context.Companies.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
        return View(new LoginViewModel { CompanyId = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewBag.Companies = await _context.Companies.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users.Include(x => x.Role).Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.CompanyId == model.CompanyId && x.UserName == model.UserName && x.IsActive);
        if (user == null || !_passwordHasher.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid company, user name, or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new("CompanyId", user.CompanyId.ToString()),
            new("DepartmentId", user.DepartmentId?.ToString() ?? string.Empty),
            new("EmployeeCode", user.EmployeeCode ?? string.Empty),
            new("EmployeeName", user.EmployeeName)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        await _audit.WriteAsync(user.CompanyId, user.Id, user.UserName, "Login", "AppUser", user.Id.ToString(), "User logged in");
        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var user = await _context.Users.FirstAsync(x => x.Id == userId);
        if (!_passwordHasher.Verify(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
            return View(model);
        }
        user.PasswordHash = _passwordHasher.Hash(model.NewPassword);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(user.CompanyId, user.Id, user.UserName, "Change Password", "AppUser", user.Id.ToString(), "Password changed by user");
        ViewBag.Message = "Password changed successfully.";
        return View(new ChangePasswordViewModel());
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
