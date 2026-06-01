-- Migration: Add Soft-Delete Support
-- Date: 2026-06-01
-- Description: Adds soft-delete columns and indexes to enable data retention compliance

-- Add soft-delete columns to AppUser
ALTER TABLE AppUser
ADD IsDeleted BIT DEFAULT 0,
    DeletedAtUtc DATETIME2 NULL,
    DeletedByUserId INT NULL;

-- Add index for soft-delete queries on AppUser
CREATE INDEX IX_AppUser_IsDeleted ON AppUser(IsDeleted);

-- Add soft-delete columns to Department
ALTER TABLE Department
ADD IsDeleted BIT DEFAULT 0,
    DeletedAtUtc DATETIME2 NULL,
    DeletedByUserId INT NULL;

-- Add index for soft-delete queries on Department
CREATE INDEX IX_Department_IsDeleted ON Department(IsDeleted);

-- Add soft-delete columns to Product
ALTER TABLE Product
ADD IsDeleted BIT DEFAULT 0,
    DeletedAtUtc DATETIME2 NULL,
    DeletedByUserId INT NULL;

-- Add index for soft-delete queries on Product
CREATE INDEX IX_Product_IsDeleted ON Product(IsDeleted);

-- Add soft-delete columns to ProductTrackingLog
ALTER TABLE ProductTrackingLog
ADD IsDeleted BIT DEFAULT 0,
    DeletedAtUtc DATETIME2 NULL,
    DeletedByUserId INT NULL;

-- Add index for soft-delete queries on ProductTrackingLog
CREATE INDEX IX_ProductTrackingLog_IsDeleted ON ProductTrackingLog(IsDeleted);
