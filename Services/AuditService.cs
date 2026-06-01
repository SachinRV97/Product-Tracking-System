using ProductTrackingSystem.Data;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Services;

public interface IAuditService
{
    Task WriteAsync(int? companyId, int? userId, string userName, string action, string? entityName = null, string? entityKey = null, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    public AuditService(ApplicationDbContext context) => _context = context;

    public async Task WriteAsync(int? companyId, int? userId, string userName, string action, string? entityName = null, string? entityKey = null, string? details = null)
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
    }
}
