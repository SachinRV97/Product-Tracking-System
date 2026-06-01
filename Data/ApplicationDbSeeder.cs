using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Models;
using ProductTrackingSystem.Services;

namespace ProductTrackingSystem.Data;

public class ApplicationDbSeeder
{
    private const string DefaultCompanyName = "Your Company Name";
    private const string AdminPassword = "Admin@123";
    private const string DemoPassword = "Demo@123";

    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbSeeder(ApplicationDbContext context, IConfiguration configuration, IPasswordHasher passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(bool seedDemoData, CancellationToken cancellationToken = default)
    {
        if (_context.Database.GetMigrations().Any())
        {
            await _context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _context.Database.EnsureCreatedAsync(cancellationToken);
        }

        var company = await EnsureCompanyAsync(cancellationToken);
        var roles = await EnsureRolesAsync(cancellationToken);
        var departments = await EnsureDepartmentsAsync(company.Id, cancellationToken);
        var users = await EnsureUsersAsync(company.Id, departments, roles, seedDemoData, cancellationToken);

        if (seedDemoData)
        {
            await EnsureDemoDataAsync(company.Id, departments, users, cancellationToken);
        }
    }

    private async Task<Company> EnsureCompanyAsync(CancellationToken cancellationToken)
    {
        var configuredName = _configuration["Company:Name"]?.Trim();
        var configuredLogo = _configuration["Company:LogoPath"]?.Trim();
        var companyName = string.IsNullOrWhiteSpace(configuredName) ? DefaultCompanyName : configuredName;

        var company = await _context.Companies
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(x => x.Name == companyName, cancellationToken)
            ?? await _context.Companies.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        if (company == null)
        {
            company = new Company
            {
                Name = companyName,
                LogoPath = string.IsNullOrWhiteSpace(configuredLogo) ? "/images/company-logo.svg" : configuredLogo,
                IsActive = true
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync(cancellationToken);
            return company;
        }

        var changed = false;
        if (!company.IsActive)
        {
            company.IsActive = true;
            changed = true;
        }

        if (company.Name == DefaultCompanyName && companyName != DefaultCompanyName)
        {
            company.Name = companyName;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(configuredLogo) && company.LogoPath != configuredLogo)
        {
            company.LogoPath = configuredLogo;
            changed = true;
        }

        if (changed)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return company;
    }

    private async Task<Dictionary<string, AppRole>> EnsureRolesAsync(CancellationToken cancellationToken)
    {
        var roleSeeds = new[]
        {
            new AppRole { Name = "Admin", Permissions = "*", IsActive = true },
            new AppRole { Name = "Manager", Permissions = "Products,Tracking,Reports,Dashboard", IsActive = true },
            new AppRole { Name = "User", Permissions = "Tracking,Dashboard", IsActive = true },
            new AppRole { Name = "Viewer", Permissions = "Reports,Dashboard", IsActive = true }
        };

        var changed = false;
        foreach (var roleSeed in roleSeeds)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == roleSeed.Name, cancellationToken);
            if (role == null)
            {
                _context.Roles.Add(roleSeed);
                changed = true;
                continue;
            }

            if (role.Permissions != roleSeed.Permissions || !role.IsActive)
            {
                role.Permissions = roleSeed.Permissions;
                role.IsActive = true;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await _context.Roles.ToDictionaryAsync(x => x.Name, cancellationToken);
    }

    private async Task<Dictionary<string, Department>> EnsureDepartmentsAsync(int companyId, CancellationToken cancellationToken)
    {
        var departmentSeeds = new[]
        {
            ("IT", "IT"),
            ("HR", "HR"),
            ("Finance", "FIN"),
            ("Administration", "ADM"),
            ("Production", "PRD"),
            ("Stores", "STR")
        };

        var changed = false;
        foreach (var (name, code) in departmentSeeds)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Name == name, cancellationToken);

            if (department == null)
            {
                _context.Departments.Add(new Department
                {
                    CompanyId = companyId,
                    Name = name,
                    Code = code,
                    IsActive = true
                });
                changed = true;
                continue;
            }

            if (department.Code != code || !department.IsActive)
            {
                department.Code = code;
                department.IsActive = true;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await _context.Departments
            .Where(x => x.CompanyId == companyId)
            .ToDictionaryAsync(x => x.Name, cancellationToken);
    }

    private async Task<Dictionary<string, AppUser>> EnsureUsersAsync(
        int companyId,
        IReadOnlyDictionary<string, Department> departments,
        IReadOnlyDictionary<string, AppRole> roles,
        bool includeDemoUsers,
        CancellationToken cancellationToken)
    {
        var userSeeds = new List<UserSeed>
        {
            new UserSeed("admin", "Master Administrator", "MASTER", "admin@company.local", "9999990000", "IT", "Admin", true, AdminPassword, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        if (includeDemoUsers)
        {
            userSeeds.AddRange(
            [
                new UserSeed("ops.manager", "Operations Manager", "EMP1001", "ops.manager@company.local", "9999991001", "Production", "Manager", false, DemoPassword, DateTime.UtcNow.AddMonths(-6)),
                new UserSeed("store.user", "Store Executive", "EMP1002", "store.user@company.local", "9999991002", "Stores", "User", false, DemoPassword, DateTime.UtcNow.AddMonths(-5)),
                new UserSeed("track.user", "Tracking Operator", "EMP1003", "track.user@company.local", "9999991003", "Production", "User", false, DemoPassword, DateTime.UtcNow.AddMonths(-4)),
                new UserSeed("viewer.user", "Report Viewer", "EMP1004", "viewer.user@company.local", "9999991004", "HR", "Viewer", false, DemoPassword, DateTime.UtcNow.AddMonths(-3)),
                new UserSeed("finance.manager", "Finance Manager", "EMP1005", "finance.manager@company.local", "9999991005", "Finance", "Manager", false, DemoPassword, DateTime.UtcNow.AddMonths(-2))
            ]);
        }

        var changed = false;
        foreach (var seed in userSeeds)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.CompanyId == companyId && x.UserName == seed.UserName,
                cancellationToken);

            if (user == null)
            {
                _context.Users.Add(new AppUser
                {
                    CompanyId = companyId,
                    DepartmentId = departments[seed.DepartmentName].Id,
                    RoleId = roles[seed.RoleName].Id,
                    UserName = seed.UserName,
                    PasswordHash = _passwordHasher.Hash(seed.Password),
                    EmployeeCode = seed.EmployeeCode,
                    EmployeeName = seed.EmployeeName,
                    Email = seed.Email,
                    Mobile = seed.Mobile,
                    IsMasterLogin = seed.IsMasterLogin,
                    IsActive = true,
                    CreatedAtUtc = seed.CreatedAtUtc
                });
                changed = true;
                continue;
            }

            if (!user.IsActive || user.DepartmentId != departments[seed.DepartmentName].Id || user.RoleId != roles[seed.RoleName].Id)
            {
                user.DepartmentId = departments[seed.DepartmentName].Id;
                user.RoleId = roles[seed.RoleName].Id;
                user.IsActive = true;
                user.IsMasterLogin = seed.IsMasterLogin;
                user.EmployeeCode ??= seed.EmployeeCode;
                user.EmployeeName = string.IsNullOrWhiteSpace(user.EmployeeName) ? seed.EmployeeName : user.EmployeeName;
                user.Email ??= seed.Email;
                user.Mobile ??= seed.Mobile;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return await _context.Users
            .Where(x => x.CompanyId == companyId)
            .ToDictionaryAsync(x => x.UserName, cancellationToken);
    }

    private async Task EnsureDemoDataAsync(
        int companyId,
        IReadOnlyDictionary<string, Department> departments,
        IReadOnlyDictionary<string, AppUser> users,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var demoProductSeeds = new[]
        {
            new DemoProductSeed("LIN-1001", "ICU Bed Linen Set", "Stores", "Linen", "CleanWave Textiles", ProductStatus.Available, LinenStage.Received, "Central Store - Rack A1", 210, 575, 28, true),
            new DemoProductSeed("LIN-1002", "Ward Pillow Cover Batch", "Production", "Linen", "BrightFab Healthcare", ProductStatus.InUse, LinenStage.Washing, "Laundry Bay 2", 180, 545, 21, true),
            new DemoProductSeed("LIN-1003", "OT Towel Pack", "Production", "Linen", "FreshFold Mills", ProductStatus.InUse, LinenStage.Ironing, "Ironing Station 1", 160, 525, 18, true),
            new DemoProductSeed("LIN-1004", "Patient Gown Set", "Administration", "Uniform", "CareTex Apparel", ProductStatus.Transferred, LinenStage.Dispatch, "Dispatch Dock 3", 150, 515, 14, true),
            new DemoProductSeed("LIN-1005", "Emergency Blanket Roll", "Stores", "Linen", "NorthStar Fabrics", ProductStatus.Available, LinenStage.Completed, "Emergency Shelf B2", 120, 485, 9, true),
            new DemoProductSeed("LIN-1006", "Isolation Apron Bundle", "Production", "Uniform", "MedWear Supplies", ProductStatus.UnderMaintenance, LinenStage.Sorting, "Quality Check Desk", 100, 465, 7, true),
            new DemoProductSeed("LIN-1007", "Premium Bed Sheet Set", "HR", "Linen", "CleanWave Textiles", ProductStatus.Transferred, LinenStage.Packing, "Training Ward Cabinet", 90, 455, 6, true),
            new DemoProductSeed("LIN-1008", "Laundry Cart Cover", "Finance", "Linen", "FreshFold Mills", ProductStatus.Disposed, LinenStage.Completed, "Scrap Clearance Bay", 365, 30, 3, false),
            new DemoProductSeed("LIN-1009", "Visitor Coat Set", "Administration", "Uniform", "CareTex Apparel", ProductStatus.InUse, LinenStage.Dispatch, "Reception Supply Room", 70, 435, 4, true),
            new DemoProductSeed("LIN-1010", "NICU Swaddle Pack", "Stores", "Linen", "NorthStar Fabrics", ProductStatus.Available, LinenStage.Packing, "Cold Storage Shelf C1", 45, 410, 2, true)
        };

        var newProducts = new List<Product>();
        foreach (var seed in demoProductSeeds)
        {
            var existing = await _context.Products.FirstOrDefaultAsync(
                x => x.CompanyId == companyId && x.TagNumber == seed.TagNumber,
                cancellationToken);

            if (existing != null)
            {
                continue;
            }

            newProducts.Add(new Product
            {
                CompanyId = companyId,
                DepartmentId = departments[seed.DepartmentName].Id,
                TagNumber = seed.TagNumber,
                ProductName = seed.ProductName,
                Category = seed.Category,
                Vendor = seed.Vendor,
                PurchaseDate = utcNow.Date.AddDays(-seed.PurchaseDaysAgo),
                WarrantyDate = utcNow.Date.AddDays(seed.WarrantyDaysFromNow),
                Status = seed.Status,
                CurrentStage = seed.Stage,
                CurrentLocation = seed.Location,
                IsActive = seed.IsActive,
                CreatedAtUtc = utcNow.AddDays(-seed.CreatedDaysAgo)
            });
        }

        if (newProducts.Count > 0)
        {
            _context.Products.AddRange(newProducts);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var products = await _context.Products
            .Where(x => x.CompanyId == companyId && demoProductSeeds.Select(seed => seed.TagNumber).Contains(x.TagNumber))
            .ToDictionaryAsync(x => x.TagNumber, cancellationToken);

        var trackingSeeds = new[]
        {
            new DemoTrackingSeed("LIN-1001", null, "Stores", ProductStatus.Available, LinenStage.Received, "Central Store - Rack A1", "Initial stock received from vendor.", "HH-01", "store.user", utcNow.AddDays(-12)),
            new DemoTrackingSeed("LIN-1001", "Stores", "Stores", ProductStatus.Available, LinenStage.Received, "Central Store - Rack A1", "Cycle count verified for opening stock.", "HH-01", "store.user", utcNow.AddHours(-2)),
            new DemoTrackingSeed("LIN-1002", null, "Stores", ProductStatus.Available, LinenStage.Received, "Inbound Shelf 4", "Pillow covers received for processing.", "HH-02", "store.user", utcNow.AddDays(-9)),
            new DemoTrackingSeed("LIN-1002", "Stores", "Production", ProductStatus.InUse, LinenStage.Washing, "Laundry Bay 2", "Batch issued to washing section.", "HH-02", "track.user", utcNow.AddDays(-1).AddHours(2)),
            new DemoTrackingSeed("LIN-1003", null, "Stores", ProductStatus.Available, LinenStage.Received, "Inbound Shelf 6", "OT towels received and counted.", "HH-03", "store.user", utcNow.AddDays(-8)),
            new DemoTrackingSeed("LIN-1003", "Stores", "Production", ProductStatus.InUse, LinenStage.Sorting, "Sorting Zone 1", "Sent for sort and stain check.", "HH-03", "track.user", utcNow.AddDays(-4)),
            new DemoTrackingSeed("LIN-1003", "Production", "Production", ProductStatus.InUse, LinenStage.Ironing, "Ironing Station 1", "Moved forward after wash completion.", "HH-03", "track.user", utcNow.AddHours(-5)),
            new DemoTrackingSeed("LIN-1004", null, "Production", ProductStatus.InUse, LinenStage.Packing, "Packing Line 2", "Patient gowns packed for dispatch.", "HH-04", "ops.manager", utcNow.AddDays(-6)),
            new DemoTrackingSeed("LIN-1004", "Production", "Administration", ProductStatus.Transferred, LinenStage.Dispatch, "Dispatch Dock 3", "Transferred to admin dispatch for ward delivery.", "HH-04", "ops.manager", utcNow.AddDays(-1)),
            new DemoTrackingSeed("LIN-1005", null, "Stores", ProductStatus.Available, LinenStage.Completed, "Emergency Shelf B2", "Emergency blanket stock verified.", "HH-01", "store.user", utcNow.AddDays(-3)),
            new DemoTrackingSeed("LIN-1006", null, "Production", ProductStatus.InUse, LinenStage.Washing, "Laundry Bay 4", "Apron bundle entered wash cycle.", "HH-05", "track.user", utcNow.AddDays(-5)),
            new DemoTrackingSeed("LIN-1006", "Production", "Production", ProductStatus.UnderMaintenance, LinenStage.Sorting, "Quality Check Desk", "Fabric tear found during inspection.", "HH-05", "ops.manager", utcNow.AddDays(-1).AddHours(4)),
            new DemoTrackingSeed("LIN-1007", null, "Stores", ProductStatus.Available, LinenStage.Completed, "Finished Goods Rack", "Sheet set completed and ready.", "HH-02", "store.user", utcNow.AddDays(-4)),
            new DemoTrackingSeed("LIN-1007", "Stores", "HR", ProductStatus.Transferred, LinenStage.Packing, "Training Ward Cabinet", "Moved for onboarding and demo usage.", "HH-02", "viewer.user", utcNow.AddHours(-1)),
            new DemoTrackingSeed("LIN-1008", null, "Finance", ProductStatus.UnderMaintenance, LinenStage.Sorting, "Damage Review Area", "Old cover flagged for repeated damage.", "HH-06", "finance.manager", utcNow.AddDays(-20)),
            new DemoTrackingSeed("LIN-1008", "Finance", "Finance", ProductStatus.Disposed, LinenStage.Completed, "Scrap Clearance Bay", "Approved for disposal after write-off.", "HH-06", "finance.manager", utcNow.AddDays(-2)),
            new DemoTrackingSeed("LIN-1009", null, "Administration", ProductStatus.InUse, LinenStage.Dispatch, "Reception Supply Room", "Visitor coats issued for front office.", "HH-07", "ops.manager", utcNow.AddDays(-3)),
            new DemoTrackingSeed("LIN-1010", null, "Stores", ProductStatus.Available, LinenStage.Packing, "Cold Storage Shelf C1", "NICU swaddle pack packed and labeled.", "HH-08", "store.user", utcNow.AddHours(-3))
        };

        var existingLogKeys = await _context.ProductTrackingLogs
            .Where(x => x.CompanyId == companyId && products.Values.Select(p => p.Id).Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.Remarks })
            .ToListAsync(cancellationToken);

        var newLogs = new List<ProductTrackingLog>();
        foreach (var seed in trackingSeeds)
        {
            var product = products[seed.TagNumber];
            if (existingLogKeys.Any(x => x.ProductId == product.Id && x.Remarks == seed.Remarks))
            {
                continue;
            }

            newLogs.Add(new ProductTrackingLog
            {
                CompanyId = companyId,
                ProductId = product.Id,
                FromDepartmentId = seed.FromDepartmentName == null ? null : departments[seed.FromDepartmentName].Id,
                ToDepartmentId = departments[seed.ToDepartmentName].Id,
                Status = seed.Status,
                Stage = seed.Stage,
                Location = seed.Location,
                Remarks = seed.Remarks,
                HandheldReaderId = seed.HandheldReaderId,
                UpdatedByUserId = users[seed.UserName].Id,
                UpdatedAtUtc = seed.UpdatedAtUtc
            });
        }

        if (newLogs.Count > 0)
        {
            _context.ProductTrackingLogs.AddRange(newLogs);
        }

        var auditSeeds = new[]
        {
            new AuditLog
            {
                CompanyId = companyId,
                UserId = users["admin"].Id,
                UserName = "admin",
                Action = "Seed Demo Data",
                EntityName = "System",
                EntityKey = "DemoSeed",
                Details = "Sample users, products, and tracking history were added for testing.",
                CreatedAtUtc = utcNow
            },
            new AuditLog
            {
                CompanyId = companyId,
                UserId = users["ops.manager"].Id,
                UserName = "ops.manager",
                Action = "Product Creation",
                EntityName = "Product",
                EntityKey = "LIN-1004",
                Details = "Patient gown set prepared for dispatch.",
                CreatedAtUtc = utcNow.AddDays(-6)
            },
            new AuditLog
            {
                CompanyId = companyId,
                UserId = users["finance.manager"].Id,
                UserName = "finance.manager",
                Action = "Tracking Entry",
                EntityName = "Product",
                EntityKey = "LIN-1008",
                Details = "Disposed damaged laundry cart cover after approval.",
                CreatedAtUtc = utcNow.AddDays(-2)
            }
        };

        foreach (var audit in auditSeeds)
        {
            var exists = await _context.AuditLogs.AnyAsync(
                x => x.CompanyId == companyId && x.Action == audit.Action && x.EntityKey == audit.EntityKey,
                cancellationToken);

            if (!exists)
            {
                _context.AuditLogs.Add(audit);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private sealed record UserSeed(
        string UserName,
        string EmployeeName,
        string EmployeeCode,
        string Email,
        string Mobile,
        string DepartmentName,
        string RoleName,
        bool IsMasterLogin,
        string Password,
        DateTime CreatedAtUtc);

    private sealed record DemoProductSeed(
        string TagNumber,
        string ProductName,
        string DepartmentName,
        string Category,
        string Vendor,
        ProductStatus Status,
        LinenStage Stage,
        string Location,
        int PurchaseDaysAgo,
        int WarrantyDaysFromNow,
        int CreatedDaysAgo,
        bool IsActive);

    private sealed record DemoTrackingSeed(
        string TagNumber,
        string? FromDepartmentName,
        string ToDepartmentName,
        ProductStatus Status,
        LinenStage Stage,
        string Location,
        string Remarks,
        string HandheldReaderId,
        string UserName,
        DateTime UpdatedAtUtc);
}
