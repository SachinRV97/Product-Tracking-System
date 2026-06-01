using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;
using ProductTrackingSystem.Services;
using ProductTrackingSystem.ViewModels;

namespace ProductTrackingSystem.Controllers;

[Authorize]
public class TrackingController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _audit;
    public TrackingController(ApplicationDbContext context, IAuditService audit) { _context = context; _audit = audit; }

    public async Task<IActionResult> Kanban()
    {
        var products = await _context.Products.Include(x => x.Department).Where(x => x.CompanyId == CurrentCompanyId && x.IsActive).ToListAsync();
        return View(products.GroupBy(x => x.CurrentStage).OrderBy(x => x.Key));
    }

    public async Task<IActionResult> Create(string? tagNumber)
    {
        await Lists();
        return View(new TrackingEntryViewModel { TagNumber = tagNumber ?? string.Empty, Status = ProductStatus.InUse, Stage = LinenStage.Received });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrackingEntryViewModel model)
    {
        await Lists();
        if (!ModelState.IsValid) return View(model);
        var product = await _context.Products.FirstOrDefaultAsync(x => x.CompanyId == CurrentCompanyId && x.TagNumber == model.TagNumber);
        if (product == null)
        {
            ModelState.AddModelError(nameof(model.TagNumber), "Tag number was not found.");
            return View(model);
        }

        var log = new ProductTrackingLog
        {
            CompanyId = CurrentCompanyId,
            ProductId = product.Id,
            FromDepartmentId = product.DepartmentId,
            ToDepartmentId = model.ToDepartmentId ?? product.DepartmentId,
            Location = model.Location,
            Status = model.Status,
            Stage = model.Stage,
            Remarks = model.Remarks,
            HandheldReaderId = model.HandheldReaderId,
            UpdatedByUserId = CurrentUserId,
            UpdatedAtUtc = DateTime.UtcNow
        };
        product.DepartmentId = model.ToDepartmentId ?? product.DepartmentId;
        product.CurrentLocation = model.Location;
        product.Status = model.Status;
        product.CurrentStage = model.Stage;
        _context.ProductTrackingLogs.Add(log);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(CurrentCompanyId, CurrentUserId, CurrentUserName, "Tracking Entry", "Product", product.TagNumber, $"Stage: {model.Stage}, Status: {model.Status}");
        return RedirectToAction(nameof(Kanban));
    }

    public async Task<IActionResult> History(string tagNumber)
    {
        var logs = await _context.ProductTrackingLogs.Include(x => x.Product).Include(x => x.FromDepartment).Include(x => x.ToDepartment).Include(x => x.UpdatedByUser)
            .Where(x => x.CompanyId == CurrentCompanyId && x.Product!.TagNumber == tagNumber)
            .OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        return View(logs);
    }

    private async Task Lists() => ViewBag.Departments = new SelectList(await _context.Departments.Where(x => x.CompanyId == CurrentCompanyId && x.IsActive).ToListAsync(), "Id", "Name");
}
