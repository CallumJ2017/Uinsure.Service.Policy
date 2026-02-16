-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Policy')
BEGIN
    CREATE DATABASE Policy;
END
GO

USE Policy;
GO

/* ===========================
   POLICIES (Aggregate Root)
   =========================== */

IF OBJECT_ID('dbo.Policies', 'U') IS NOT NULL
    DROP TABLE dbo.Policies;
GO

CREATE TABLE Policies (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,

    Reference NVARCHAR(50) NOT NULL,
    InsuranceType INT NOT NULL,
    Status INT NOT NULL,

    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,

    Premium DECIMAL(18,2) NOT NULL,

    AutoRenew BIT NOT NULL,
    HasClaims BIT NOT NULL,

    CreatedAt DATETIMEOFFSET NOT NULL,
    LastModifiedAt DATETIMEOFFSET NULL
);
GO


/* ===========================
   PROPERTIES (1:1 with Policy)
   =========================== */

IF OBJECT_ID('dbo.Properties', 'U') IS NOT NULL
    DROP TABLE dbo.Properties;
GO

CREATE TABLE Properties (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PolicyId UNIQUEIDENTIFIER NOT NULL UNIQUE,

    AddressLine1 NVARCHAR(255) NOT NULL,
    AddressLine2 NVARCHAR(255) NULL,
    AddressLine3 NVARCHAR(255) NULL,
    Postcode NVARCHAR(8) NOT NULL,

    CONSTRAINT FK_Properties_Policies
        FOREIGN KEY (PolicyId)
        REFERENCES Policies(Id)
        ON DELETE CASCADE
);
GO


/* ===========================
   POLICYHOLDERS (1:M with Policy)
   =========================== */

IF OBJECT_ID('dbo.Policyholders', 'U') IS NOT NULL
    DROP TABLE dbo.Policyholders;
GO

CREATE TABLE Policyholders (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PolicyId UNIQUEIDENTIFIER NOT NULL,

    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,

    CONSTRAINT FK_Policyholders_Policies
        FOREIGN KEY (PolicyId)
        REFERENCES Policies(Id)
        ON DELETE CASCADE
);
GO


/* ===========================
   Indexes
   =========================== */

CREATE INDEX IX_Policyholders_PolicyId ON Policyholders(PolicyId);
CREATE UNIQUE INDEX IX_Properties_PolicyId ON Properties(PolicyId);
GO