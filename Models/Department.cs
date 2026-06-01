using System.ComponentModel.DataAnnotations;
using ProductTrackingSystem.Models.Base;

namespace ProductTrackingSystem.Models;

public class Department : BaseEntity
{
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    [MaxLength(30)] public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
