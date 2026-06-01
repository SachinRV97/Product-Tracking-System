using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class AppUser
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int RoleId { get; set; }
    public AppRole? Role { get; set; }
    [Required, MaxLength(80)] public string UserName { get; set; } = string.Empty;
    [Required, MaxLength(160)] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(30)] public string? EmployeeCode { get; set; }
    [Required, MaxLength(120)] public string EmployeeName { get; set; } = string.Empty;
    [EmailAddress, MaxLength(120)] public string? Email { get; set; }
    [MaxLength(20)] public string? Mobile { get; set; }
    public bool IsMasterLogin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
