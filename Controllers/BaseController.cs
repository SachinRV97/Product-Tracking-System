using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ProductTrackingSystem.Controllers;

public abstract class BaseController : Controller
{
    protected int CurrentCompanyId => int.Parse(User.FindFirstValue("CompanyId") ?? "1");
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    protected string CurrentUserName => User.Identity?.Name ?? "system";
}
