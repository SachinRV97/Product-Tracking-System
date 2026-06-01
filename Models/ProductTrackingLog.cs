using System.ComponentModel.DataAnnotations;
using ProductTrackingSystem.Models.Base;

namespace ProductTrackingSystem.Models;

public class ProductTrackingLog : BaseEntity
{
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? FromDepartmentId { get; set; }
    public Department? FromDepartment { get; set; }
    public int? ToDepartmentId { get; set; }
    public Department? ToDepartment { get; set; }
    public ProductStatus Status { get; set; }
    public LinenStage Stage { get; set; }
    [MaxLength(160)] public string? Location { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
    [MaxLength(40)] public string? HandheldReaderId { get; set; }
    public int UpdatedByUserId { get; set; }
    public AppUser? UpdatedByUser { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
