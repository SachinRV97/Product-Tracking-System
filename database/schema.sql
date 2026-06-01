CREATE DATABASE ProductTrackingSystem;
GO
USE ProductTrackingSystem;
GO
CREATE TABLE Companies (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(160) NOT NULL UNIQUE,
    LogoPath NVARCHAR(260) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
CREATE TABLE Roles (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(60) NOT NULL UNIQUE,
    Permissions NVARCHAR(600) NOT NULL DEFAULT '',
    IsActive BIT NOT NULL DEFAULT 1
);
CREATE TABLE Departments (
    Id INT IDENTITY PRIMARY KEY,
    CompanyId INT NOT NULL REFERENCES Companies(Id),
    Name NVARCHAR(80) NOT NULL,
    Code NVARCHAR(30) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UX_Departments_Company_Name UNIQUE (CompanyId, Name)
);
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    CompanyId INT NOT NULL REFERENCES Companies(Id),
    DepartmentId INT NULL REFERENCES Departments(Id),
    RoleId INT NOT NULL REFERENCES Roles(Id),
    UserName NVARCHAR(80) NOT NULL,
    PasswordHash NVARCHAR(160) NOT NULL,
    EmployeeCode NVARCHAR(30) NULL,
    EmployeeName NVARCHAR(120) NOT NULL,
    Email NVARCHAR(120) NULL,
    Mobile NVARCHAR(20) NULL,
    IsMasterLogin BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UX_Users_Company_UserName UNIQUE (CompanyId, UserName)
);
CREATE TABLE Products (
    Id INT IDENTITY PRIMARY KEY,
    CompanyId INT NOT NULL REFERENCES Companies(Id),
    DepartmentId INT NOT NULL REFERENCES Departments(Id),
    TagNumber NVARCHAR(40) NOT NULL,
    ProductName NVARCHAR(160) NOT NULL,
    Category NVARCHAR(80) NULL,
    Vendor NVARCHAR(120) NULL,
    PurchaseDate DATETIME2 NULL,
    WarrantyDate DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 1,
    CurrentStage INT NOT NULL DEFAULT 1,
    CurrentLocation NVARCHAR(160) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UX_Products_Company_TagNumber UNIQUE (CompanyId, TagNumber)
);
CREATE TABLE ProductTrackingLogs (
    Id BIGINT IDENTITY PRIMARY KEY,
    CompanyId INT NOT NULL REFERENCES Companies(Id),
    ProductId INT NOT NULL REFERENCES Products(Id),
    FromDepartmentId INT NULL REFERENCES Departments(Id),
    ToDepartmentId INT NULL REFERENCES Departments(Id),
    Status INT NOT NULL,
    Stage INT NOT NULL,
    Location NVARCHAR(160) NULL,
    Remarks NVARCHAR(500) NULL,
    HandheldReaderId NVARCHAR(40) NULL,
    UpdatedByUserId INT NOT NULL REFERENCES Users(Id),
    UpdatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
CREATE INDEX IX_ProductTrackingLogs_UpdatedAtUtc ON ProductTrackingLogs(UpdatedAtUtc);
CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY PRIMARY KEY,
    CompanyId INT NULL,
    UserId INT NULL,
    UserName NVARCHAR(80) NOT NULL,
    Action NVARCHAR(80) NOT NULL,
    EntityName NVARCHAR(80) NULL,
    EntityKey NVARCHAR(60) NULL,
    Details NVARCHAR(1000) NULL,
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
CREATE INDEX IX_AuditLogs_CreatedAtUtc ON AuditLogs(CreatedAtUtc);
