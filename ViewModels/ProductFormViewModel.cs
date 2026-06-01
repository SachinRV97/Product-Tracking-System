using ProductTrackingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }
    public string? TagNumber { get; set; }
    [Required] public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    [Required] public int DepartmentId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public LinenStage CurrentStage { get; set; } = LinenStage.Received;
    public string? CurrentLocation { get; set; }
}
