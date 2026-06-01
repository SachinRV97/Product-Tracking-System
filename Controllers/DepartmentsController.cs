using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;
using ProductTrackingSystem.Services;

namespace ProductTrackingSystem.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class DepartmentsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _audit;
    public DepartmentsController(ApplicationDbContext context, IAuditService audit) { _context = context; _audit = audit; }

    public async Task<IActionResult> Index() => View(await _context.Departments.Where(x => x.CompanyId == CurrentCompanyId).OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View("Edit", new Department { CompanyId = CurrentCompanyId, IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Department department)
    {
        if (!ModelState.IsValid) return View(department);
        department.CompanyId = CurrentCompanyId;
        if (department.Id == 0) _context.Departments.Add(department); else _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(CurrentCompanyId, CurrentUserId, CurrentUserName, "Save Department", "Department", department.Id.ToString(), department.Name);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _context.Departments.FirstAsync(x => x.Id == id && x.CompanyId == CurrentCompanyId));
}
