using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.Models;

public class AuditLog
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public int? UserId { get; set; }
    [Required, MaxLength(80)] public string UserName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string Action { get; set; } = string.Empty;
    [MaxLength(80)] public string? EntityName { get; set; }
    [MaxLength(60)] public string? EntityKey { get; set; }
    [MaxLength(1000)] public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
