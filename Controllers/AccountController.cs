using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Infrastructure.Constants;
using ProductTrackingSystem.Services;
using ProductTrackingSystem.Utilities;
using ProductTrackingSystem.ViewModels;
using System.Security.Claims;

namespace ProductTrackingSystem.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApplicationDbContext context, IPasswordHasher passwordHasher, IAuditService audit, ILogger<AccountController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _audit = audit;
        _logger = logger;
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
            _logger.LogWarning("Failed login attempt for user {UserName} in company {CompanyId}", model.UserName, model.CompanyId);
            ModelState.AddModelError(string.Empty, "Invalid company, user name, or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role?.Name ?? AppConstants.Roles.User),
            new(AppConstants.Claims.CompanyId, user.CompanyId.ToString()),
            new(AppConstants.Claims.DepartmentId, user.DepartmentId?.ToString() ?? string.Empty),
            new(AppConstants.Claims.EmployeeCode, user.EmployeeCode ?? string.Empty),
            new(AppConstants.Claims.EmployeeName, user.EmployeeName)
        };
        
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        
        await _audit.WriteAsync(user.CompanyId, user.Id, user.UserName, AppConstants.Audit.ActionLogin, "AppUser", user.Id.ToString(), "User logged in");
        
        _logger.LogInformation("User {UserName} logged in successfully", user.UserName);
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
        
        var userId = User.GetUserId();
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid user ID for password change");
            return RedirectToAction(nameof(Logout));
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for password change", userId);
            return RedirectToAction(nameof(Logout));
        }

        if (!_passwordHasher.Verify(model.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Current password incorrect for user {UserName}", user.UserName);
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
            return View(model);
        }

        user.PasswordHash = _passwordHasher.Hash(model.NewPassword);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(user.CompanyId, user.Id, user.UserName, AppConstants.Audit.ActionChangePassword, "AppUser", user.Id.ToString(), "Password changed by user");
        
        _logger.LogInformation("User {UserName} changed password successfully", user.UserName);
        ViewBag.Message = "Password changed successfully.";
        return View(new ChangePasswordViewModel());
    }

    public async Task<IActionResult> Logout()
    {
        var userName = User.GetUserName();
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();
        
        await HttpContext.SignOutAsync();
        
        if (userId > 0)
        {
            await _audit.WriteAsync(companyId, userId, userName, AppConstants.Audit.ActionLogout, "AppUser", userId.ToString(), "User logged out");
        }
        
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
