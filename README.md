# Product Tracking System

ASP.NET Core MVC + Microsoft SQL Server starter implementation for a company-wise product and linen tracking system.

## Implemented modules

- Company-wise login with one seeded master admin and support for additional users.
- Role management with Admin, Manager, User, and Viewer permissions.
- User management with employee code, employee name, department, contact details, active status, and password reset fields.
- Dashboard widgets for product, user, department, and tracking summaries.
- Department master data.
- Product master with SQL-enforced unique tag numbers per company.
- Product tracking log book with mandatory UTC timestamp, user, department movement, stage, status, reader ID, and remarks.
- Linen Kanban board by process stage for handheld-reader-driven operations.
- Printable reports for products, tracking, and department totals.
- Audit logging service for login, product creation, department updates, and tracking entries.

## Default setup

1. Update `ConnectionStrings:DefaultConnection` in `appsettings.json` for your SQL Server.
2. Run EF Core migrations or apply the SQL reference schema in `database/schema.sql`.
3. Login with the seeded master account:
   - Company: `Your Company Name`
   - User name: `admin`
   - Password: `Admin@123`
4. Replace `Company:Name` and `Company:LogoPath` in `appsettings.json` and upload your company logo under `wwwroot/images`.

## Data retention

The configuration key `Company:DataRetentionDays` is set to 365 days to match the requirement that data remains in the system for at least one year from the date of log. Production deployments should archive rather than delete operational/audit rows before this period expires.
