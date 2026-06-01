using Microsoft.AspNetCore.Mvc;
using ProductTrackingSystem.Utilities;

namespace ProductTrackingSystem.Controllers;

/// <summary>
/// Base controller with safe claims extraction and common functionality
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>Safe extraction of current company ID from claims</summary>
    protected int CurrentCompanyId => User.GetCompanyId();
    
    /// <summary>Safe extraction of current user ID from claims</summary>
    protected int CurrentUserId => User.GetUserId();
    
    /// <summary>Safe extraction of current username from identity</summary>
    protected string CurrentUserName => User.GetUserName();
    
    /// <summary>Safe extraction of current department ID from claims</summary>
    protected int? CurrentDepartmentId => User.GetDepartmentId();
    
    /// <summary>Check if user is properly authenticated</summary>
    protected bool IsAuthenticated => User.IsAuthenticated();
}
