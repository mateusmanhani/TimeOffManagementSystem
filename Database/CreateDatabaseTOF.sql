-- Create the Core database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MAG.TOF')
BEGIN
    EXEC('CREATE DATABASE [MAG.TOF];')
END
GO

USE [MAG.TOF];

GO

CREATE TABLE Requests (
    Id INT IDENTITY(1,1) NOT NULL,
    
	-- Reference to User Id and Department Id in Core DB
    UserId INT NOT NULL,
    DepartmentId INT NOT NULL,

    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    TotalBusinessDays INT NOT NULL,

	-- Reference to User Id (Manager) in Core DB
    ManagerId INT NOT NULL,
    ManagerComment NVARCHAR(1000) NULL,
    
    -- StatusId validation
    StatusId INT NOT NULL DEFAULT 1,

    CONSTRAINT PK_Requests PRIMARY KEY CLUSTERED (Id),

    -- Internal Constraints
	-- Ensure status id are within range
    CONSTRAINT CK_Requests_StatusId CHECK (StatusId IN (1, 2, 3, 4, 5)),
	-- ensure end date is later than the start date
    CONSTRAINT CK_Requests_Dates CHECK (EndDate >= StartDate),
	-- ensure that if status is rejected (4) a manager comment is required
    CONSTRAINT CK_Requests_ManagerComment_Mandatory_If_Rejected 
        CHECK (StatusId <> 4 OR (StatusId = 4 AND ManagerComment IS NOT NULL AND LEN(ManagerComment) > 0))
);
GO

-- Since we don't have FK constraints, we should manually add Non-Clustered Indexes
-- so that queries like "Get all requests for User X" are fast.
CREATE NONCLUSTERED INDEX IX_Requests_UserId ON Requests(UserId);
CREATE NONCLUSTERED INDEX IX_Requests_DepartmentId ON Requests(DepartmentId);
CREATE NONCLUSTERED INDEX IX_Requests_ManagerId ON Requests(ManagerId);

CREATE TABLE RequestLogs(
	Id INT IDENTITY(1,1) NOT NULL,
	RequestId INT NULL,
	LogDate DATETIME NOT NULL DEFAULT GETDATE(),
	LogLeverl NVARCHAR(50) NOT NULL,
	LogMessage NVARCHAR(MAX) NULL,
	LogException NVARCHAR(MAX) NULL, -- Stores the stack trace if there's a crash
    Logger NVARCHAR(255) NULL,       -- Stores which class/controller sent the log
	
	CONSTRAINT	PK_RequestLogs PRIMARY KEY CLUSTERED (Id)
);
GO

-- Index for fast lookups when you want to see "The Story" of a specific request
CREATE NONCLUSTERED INDEX IX_RequestLogs_RequestId ON RequestLogs(RequestId);
GO