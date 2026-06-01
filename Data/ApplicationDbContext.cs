using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Data;

/// <summary>
/// Application database context with soft-delete support and query filtering
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTrackingLog> ProductTrackingLogs => Set<ProductTrackingLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure soft-delete query filters to exclude deleted records by default
        ConfigureSoftDeleteQueryFilters(modelBuilder);

        // Indexes
        modelBuilder.Entity<Company>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Department>().HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
        modelBuilder.Entity<AppRole>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(x => new { x.CompanyId, x.UserName }).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => new { x.CompanyId, x.TagNumber }).IsUnique();
        modelBuilder.Entity<ProductTrackingLog>().HasIndex(x => x.UpdatedAtUtc);
        modelBuilder.Entity<AuditLog>().HasIndex(x => x.CreatedAtUtc);
        
        // Add indexes for soft-delete queries
        modelBuilder.Entity<AppUser>().HasIndex(x => x.IsDeleted);
        modelBuilder.Entity<Department>().HasIndex(x => x.IsDeleted);
        modelBuilder.Entity<Product>().HasIndex(x => x.IsDeleted);
        modelBuilder.Entity<ProductTrackingLog>().HasIndex(x => x.IsDeleted);

        // Configure foreign keys
        modelBuilder.Entity<ProductTrackingLog>()
            .HasOne(x => x.FromDepartment)
            .WithMany()
            .HasForeignKey(x => x.FromDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProductTrackingLog>()
            .HasOne(x => x.ToDepartment)
            .WithMany()
            .HasForeignKey(x => x.ToDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProductTrackingLog>()
            .HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial data
        modelBuilder.Entity<Company>().HasData(new Company { Id = 1, Name = "Your Company Name", LogoPath = "/images/company-logo.svg", IsActive = true });
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin", Permissions = "*", IsActive = true },
            new AppRole { Id = 2, Name = "Manager", Permissions = "Products,Tracking,Reports,Dashboard", IsActive = true },
            new AppRole { Id = 3, Name = "User", Permissions = "Tracking,Dashboard", IsActive = true },
            new AppRole { Id = 4, Name = "Viewer", Permissions = "Reports,Dashboard", IsActive = true });
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, CompanyId = 1, Name = "IT", Code = "IT", IsActive = true },
            new Department { Id = 2, CompanyId = 1, Name = "HR", Code = "HR", IsActive = true },
            new Department { Id = 3, CompanyId = 1, Name = "Finance", Code = "FIN", IsActive = true },
            new Department { Id = 4, CompanyId = 1, Name = "Administration", Code = "ADM", IsActive = true },
            new Department { Id = 5, CompanyId = 1, Name = "Production", Code = "PRD", IsActive = true },
            new Department { Id = 6, CompanyId = 1, Name = "Stores", Code = "STR", IsActive = true });

        modelBuilder.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            CompanyId = 1,
            DepartmentId = 1,
            RoleId = 1,
            UserName = "admin",
            PasswordHash = "100000.UHJvZHVjdFRyYWNrU2FsdA==.Xqa4mhgYVetUvaNSIWZHoDvdXG4Br6MFbhguc+hDkZE=",
            EmployeeCode = "MASTER",
            EmployeeName = "Master Administrator",
            Email = "admin@company.local",
            IsMasterLogin = true,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    /// <summary>
    /// Configure query filters to automatically exclude soft-deleted records
    /// </summary>
    private void ConfigureSoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        // Filter AppUser - exclude deleted
        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(u => !u.IsDeleted);

        // Filter Department - exclude deleted
        modelBuilder.Entity<Department>()
            .HasQueryFilter(d => !d.IsDeleted);

        // Filter Product - exclude deleted
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);

        // Filter ProductTrackingLog - exclude deleted
        modelBuilder.Entity<ProductTrackingLog>()
            .HasQueryFilter(ptl => !ptl.IsDeleted);
    }
}
