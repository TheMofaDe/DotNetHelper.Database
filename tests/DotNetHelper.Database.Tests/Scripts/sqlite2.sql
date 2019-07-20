CREATE TABLE IF NOT EXISTS [Employee2] (
	[IdentityField] [INTEGER] NOT NULL PRIMARY KEY AUTOINCREMENT,
	[EmployeeListAsJson] VARCHAR(400)  NULL,
	[EmployeeAsJson] VARCHAR(400)  NULL,
	[EmployeeListAsCsv] VARCHAR(400)  NULL,
	[EmployeeAsCsv] VARCHAR(400)  NULL,
	[EmployeeListAsXml] VARCHAR(400)  NULL,
	[EmployeeAsXml] VARCHAR(400)  NULL
);