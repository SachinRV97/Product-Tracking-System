using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;
using ProductTrackingSystem.Services;
using ProductTrackingSystem.ViewModels;

namespace ProductTrackingSystem.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ProductsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITagNumberService _tagService;
    private readonly IAuditService _audit;
    public ProductsController(ApplicationDbContext context, ITagNumberService tagService, IAuditService audit) { _context = context; _tagService = tagService; _audit = audit; }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Products.Include(x => x.Department).Where(x => x.CompanyId == CurrentCompanyId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.TagNumber.Contains(search) || x.ProductName.Contains(search));
        return View(await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync());
    }

    public async Task<IActionResult> Create() { await Lists(); return View(new ProductFormViewModel { TagNumber = await _tagService.GenerateAsync(CurrentCompanyId) }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        await Lists();
        if (!ModelState.IsValid) return View(model);
        var product = new Product
        {
            CompanyId = CurrentCompanyId,
            DepartmentId = model.DepartmentId,
            TagNumber = string.IsNullOrWhiteSpace(model.TagNumber) ? await _tagService.GenerateAsync(CurrentCompanyId) : model.TagNumber,
            ProductName = model.ProductName,
            Category = model.Category,
            Vendor = model.Vendor,
            PurchaseDate = model.PurchaseDate,
            WarrantyDate = model.WarrantyDate,
            Status = model.Status,
            CurrentStage = model.CurrentStage,
            CurrentLocation = model.CurrentLocation
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(CurrentCompanyId, CurrentUserId, CurrentUserName, "Create Product", "Product", product.TagNumber, product.ProductName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        await Lists();
        var product = await _context.Products.FirstAsync(x => x.Id == id && x.CompanyId == CurrentCompanyId);
        return View(new ProductFormViewModel
        {
            Id = product.Id,
            TagNumber = product.TagNumber,
            ProductName = product.ProductName,
            Category = product.Category,
            DepartmentId = product.DepartmentId,
            PurchaseDate = product.PurchaseDate,
            Vendor = product.Vendor,
            WarrantyDate = product.WarrantyDate,
            Status = product.Status,
            CurrentStage = product.CurrentStage,
            CurrentLocation = product.CurrentLocation
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        await Lists();
        if (!ModelState.IsValid) return View(model);
        var product = await _context.Products.FirstAsync(x => x.Id == model.Id && x.CompanyId == CurrentCompanyId);
        product.ProductName = model.ProductName;
        product.Category = model.Category;
        product.DepartmentId = model.DepartmentId;
        product.PurchaseDate = model.PurchaseDate;
        product.Vendor = model.Vendor;
        product.WarrantyDate = model.WarrantyDate;
        product.Status = model.Status;
        product.CurrentStage = model.CurrentStage;
        product.CurrentLocation = model.CurrentLocation;
        await _context.SaveChangesAsync();
        await _audit.WriteAsync(CurrentCompanyId, CurrentUserId, CurrentUserName, "Update Product", "Product", product.TagNumber, product.ProductName);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.Include(x => x.Department).Include(x => x.TrackingLogs).ThenInclude(x => x.UpdatedByUser)
            .FirstAsync(x => x.Id == id && x.CompanyId == CurrentCompanyId);
        return View(product);
    }

    private async Task Lists() => ViewBag.Departments = new SelectList(await _context.Departments.Where(x => x.CompanyId == CurrentCompanyId && x.IsActive).ToListAsync(), "Id", "Name");
}
