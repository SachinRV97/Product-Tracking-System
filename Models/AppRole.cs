using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class AppRole
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string Name { get; set; } = string.Empty;
    [MaxLength(600)] public string Permissions { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
