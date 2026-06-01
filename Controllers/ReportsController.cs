using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;

namespace ProductTrackingSystem.Controllers;

[Authorize(Roles = "Admin,Manager,Viewer")]
public class ReportsController : BaseController
{
    private readonly ApplicationDbContext _context;
    public ReportsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? status)
    {
        var start = from ?? DateTime.UtcNow.AddYears(-1);
        var end = (to ?? DateTime.UtcNow).Date.AddDays(1);
        ViewBag.Products = await _context.Products.Include(x => x.Department).Where(x => x.CompanyId == CurrentCompanyId).ToListAsync();
        ViewBag.Tracking = await _context.ProductTrackingLogs.Include(x => x.Product).Include(x => x.ToDepartment).Include(x => x.UpdatedByUser)
            .Where(x => x.CompanyId == CurrentCompanyId && x.UpdatedAtUtc >= start && x.UpdatedAtUtc < end)
            .OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        ViewBag.DepartmentSummary = await _context.Products.Include(x => x.Department).Where(x => x.CompanyId == CurrentCompanyId)
            .GroupBy(x => x.Department!.Name).Select(x => new { Department = x.Key, TotalProducts = x.Count() }).ToListAsync();
        return View();
    }
}
