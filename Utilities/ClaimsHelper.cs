namespace ProductTrackingSystem.Utilities;

using System.Security.Claims;

/// <summary>
/// Safe claims extraction helper with null-safety
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Safely extract integer claim value with fallback to default
    /// </summary>
    public static int GetClaimInt(this ClaimsPrincipal user, string claimType, int defaultValue = 0)
    {
        var claim = user?.FindFirstValue(claimType);
        return int.TryParse(claim, out var value) ? value : defaultValue;
    }
    
    /// <summary>
    /// Safely extract string claim value
    /// </summary>
    public static string GetClaimString(this ClaimsPrincipal user, string claimType, string defaultValue = "")
    {
        return user?.FindFirstValue(claimType) ?? defaultValue;
    }
    
    /// <summary>
    /// Extract company ID from claims
    /// </summary>
    public static int GetCompanyId(this ClaimsPrincipal user) =>
        user.GetClaimInt("CompanyId", 1);
    
    /// <summary>
    /// Extract user ID from identity
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        user.GetClaimInt(ClaimTypes.NameIdentifier, 0);
    
    /// <summary>
    /// Extract department ID from claims
    /// </summary>
    public static int? GetDepartmentId(this ClaimsPrincipal user)
    {
        var deptId = user?.FindFirstValue("DepartmentId");
        return int.TryParse(deptId, out var value) && value > 0 ? value : null;
    }
    
    /// <summary>
    /// Extract username from identity
    /// </summary>
    public static string GetUserName(this ClaimsPrincipal user) =>
        user?.Identity?.Name ?? "system";
    
    /// <summary>
    /// Check if user ID is valid (non-zero)
    /// </summary>
    public static bool IsAuthenticated(this ClaimsPrincipal user) =>
        user?.GetUserId() > 0;
}
