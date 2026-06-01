using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;
using ProductTrackingSystem.Services;

namespace ProductTrackingSystem.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public UsersController(ApplicationDbContext context, IPasswordHasher passwordHasher) { _context = context; _passwordHasher = passwordHasher; }

    public async Task<IActionResult> Index() => View(await _context.Users.Include(x => x.Department).Include(x => x.Role).Where(x => x.CompanyId == CurrentCompanyId).ToListAsync());
    public async Task<IActionResult> Create() { await Lists(); return View("Edit", new AppUser { CompanyId = CurrentCompanyId, IsActive = true }); }
    public async Task<IActionResult> Edit(int id) { await Lists(); return View(await _context.Users.FirstAsync(x => x.Id == id && x.CompanyId == CurrentCompanyId)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppUser user, string? newPassword)
    {
        await Lists();
        ModelState.Remove(nameof(AppUser.PasswordHash));
        if (user.Id == 0 && string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError(nameof(newPassword), "Password is required for new users.");
        }
        if (!ModelState.IsValid) return View(user);
        user.CompanyId = CurrentCompanyId;
        if (!string.IsNullOrWhiteSpace(newPassword)) user.PasswordHash = _passwordHasher.Hash(newPassword);
        if (user.Id == 0) _context.Users.Add(user); else _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task Lists()
    {
        ViewBag.Departments = new SelectList(await _context.Departments.Where(x => x.CompanyId == CurrentCompanyId && x.IsActive).ToListAsync(), "Id", "Name");
        ViewBag.Roles = new SelectList(await _context.Roles.Where(x => x.IsActive).ToListAsync(), "Id", "Name");
    }
}
