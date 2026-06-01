namespace ProductTrackingSystem.Infrastructure.Constants;

/// <summary>
/// Centralized constants for roles, permissions, and claims
/// </summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
        public const string Viewer = "Viewer";
        
        public static readonly string[] All = [Admin, Manager, User, Viewer];
    }
    
    public static class Permissions
    {
        public const string AdminAll = "*";
        public const string Products = "Products";
        public const string Tracking = "Tracking";
        public const string Reports = "Reports";
        public const string Users = "Users";
        public const string Dashboard = "Dashboard";
        public const string Departments = "Departments";
        public const string Roles = "Roles";
    }
    
    public static class Claims
    {
        public const string CompanyId = "CompanyId";
        public const string DepartmentId = "DepartmentId";
        public const string EmployeeCode = "EmployeeCode";
        public const string EmployeeName = "EmployeeName";
    }
    
    public static class Audit
    {
        public const string ActionLogin = "Login";
        public const string ActionLogout = "Logout";
        public const string ActionCreate = "Create";
        public const string ActionUpdate = "Update";
        public const string ActionDelete = "Delete";
        public const string ActionChangePassword = "Change Password";
    }
}
