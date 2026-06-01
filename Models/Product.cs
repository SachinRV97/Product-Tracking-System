using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class Product
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    [Required, MaxLength(40)] public string TagNumber { get; set; } = string.Empty;
    [Required, MaxLength(160)] public string ProductName { get; set; } = string.Empty;
    [MaxLength(80)] public string? Category { get; set; }
    [MaxLength(120)] public string? Vendor { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public LinenStage CurrentStage { get; set; } = LinenStage.Received;
    [MaxLength(160)] public string? CurrentLocation { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ProductTrackingLog> TrackingLogs { get; set; } = new List<ProductTrackingLog>();
}
