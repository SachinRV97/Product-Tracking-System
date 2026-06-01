namespace ProductTrackingSystem.Models.Base;

/// <summary>
/// Base entity class providing soft-delete and audit trail support
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    
    /// <summary>Soft-delete flag for compliance with data retention policies</summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>UTC timestamp when entity was soft-deleted</summary>
    public DateTime? DeletedAtUtc { get; set; }
    
    /// <summary>User ID who deleted the entity</summary>
    public int? DeletedByUserId { get; set; }
}
