using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class Company
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Name { get; set; } = string.Empty;
    [MaxLength(260)] public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
