using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class Department
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    [MaxLength(30)] public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
