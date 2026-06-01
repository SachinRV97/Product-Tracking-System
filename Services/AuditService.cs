using ProductTrackingSystem.Data;
using ProductTrackingSystem.Infrastructure.Constants;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Services;

/// <summary>
/// Immutable audit logging service for compliance and security tracking
/// </summary>
public interface IAuditService
{
    Task WriteAsync(int? companyId, int? userId, string userName, string action, string? entityName = null, string? entityKey = null, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task WriteAsync(int? companyId, int? userId, string userName, string action, string? entityName = null, string? entityKey = null, string? details = null)
    {
        try
        {
            _context.AuditLogs.Add(new AuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                EntityKey = entityKey,
                Details = details,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("Audit logged: {Action} by {UserName} for {EntityName}", action, userName, entityName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for action: {Action}", action);
            throw;
        }
    }
}
