namespace ProductTrackingSystem.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;
using ProductTrackingSystem.Utilities;

/// <summary>
/// Resource-level authorization handler to enforce company/department context
/// </summary>
public class ResourceAuthorizationHandler : AuthorizationHandler<ResourceAuthorizationRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResourceAuthorizationHandler> _logger;
    
    public ResourceAuthorizationHandler(ApplicationDbContext context, ILogger<ResourceAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceAuthorizationRequirement requirement)
    {
        var userId = context.User.GetUserId();
        var companyId = context.User.GetCompanyId();
        
        if (userId <= 0 || companyId <= 0)
        {
            _logger.LogWarning("Invalid claims for authorization: UserId={UserId}, CompanyId={CompanyId}", userId, companyId);
            return;
        }
        
        // Verify user exists and belongs to the specified company
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId && u.IsActive && !u.IsDeleted);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found in company {CompanyId}", userId, companyId);
            return;
        }
        
        // For department-specific resources, verify user's department access
        if (requirement.RequireDepartmentAccess && user.DepartmentId.HasValue)
        {
            var dept = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == user.DepartmentId && d.CompanyId == companyId && d.IsActive && !d.IsDeleted);
            
            if (dept == null)
            {
                _logger.LogWarning("User {UserId} department not found", userId);
                return;
            }
        }
        
        context.Succeed(requirement);
    }
}

/// <summary>
/// Authorization requirement for resource-level checks
/// </summary>
public class ResourceAuthorizationRequirement : IAuthorizationRequirement
{
    public bool RequireDepartmentAccess { get; set; } = false;
}
