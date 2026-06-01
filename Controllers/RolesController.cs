using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly ApplicationDbContext _context;
    public RolesController(ApplicationDbContext context) => _context = context;
    public async Task<IActionResult> Index() => View(await _context.Roles.OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View("Edit", new AppRole { IsActive = true });
    public async Task<IActionResult> Edit(int id) => View(await _context.Roles.FirstAsync(x => x.Id == id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppRole role)
    {
        if (!ModelState.IsValid) return View(role);
        if (role.Id == 0) _context.Roles.Add(role); else _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
