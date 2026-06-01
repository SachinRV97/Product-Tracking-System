using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        ViewBag.TotalProducts = await _context.Products.CountAsync(x => x.CompanyId == CurrentCompanyId);
        ViewBag.ActiveProducts = await _context.Products.CountAsync(x => x.CompanyId == CurrentCompanyId && x.IsActive);
        ViewBag.TransferredProducts = await _context.Products.CountAsync(x => x.CompanyId == CurrentCompanyId && x.Status == ProductStatus.Transferred);
        ViewBag.DisposedProducts = await _context.Products.CountAsync(x => x.CompanyId == CurrentCompanyId && x.Status == ProductStatus.Disposed);
        ViewBag.TotalUsers = await _context.Users.CountAsync(x => x.CompanyId == CurrentCompanyId);
        ViewBag.ActiveUsers = await _context.Users.CountAsync(x => x.CompanyId == CurrentCompanyId && x.IsActive);
        ViewBag.DepartmentCount = await _context.Departments.CountAsync(x => x.CompanyId == CurrentCompanyId && x.IsActive);
        ViewBag.TodayTracking = await _context.ProductTrackingLogs.CountAsync(x => x.CompanyId == CurrentCompanyId && x.UpdatedAtUtc >= today);
        ViewBag.ByDepartment = await _context.Products.Include(x => x.Department)
            .Where(x => x.CompanyId == CurrentCompanyId)
            .GroupBy(x => x.Department!.Name)
            .Select(x => new { Name = x.Key, Count = x.Count() }).ToListAsync();
        ViewBag.ByStatus = await _context.Products.Where(x => x.CompanyId == CurrentCompanyId)
            .GroupBy(x => x.Status)
            .Select(x => new { Name = x.Key.ToString(), Count = x.Count() }).ToListAsync();
        return View();
    }
}
