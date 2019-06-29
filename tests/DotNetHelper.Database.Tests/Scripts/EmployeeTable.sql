IF DB_ID (N'Test') IS NOT NULL
DROP DATABASE Test;
GO
CREATE DATABASE Test;
GO
CREATE TABLE [Test].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1),
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DateOfBirth] DateTime NOT NULL,
	[CreatedAt] DateTime NOT NULL,
	[FavoriteColor] [varchar](400) NOT NULL
);
CREATE TABLE [Test].[dbo].[Employee2](
	[IdentityField] [int] NOT NULL IDENTITY (1,1),
	[PrimaryKey] [varchar](400) NOT NULL PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DateOfBirth] DateTime NOT NULL,
	[CreatedAt] DateTime NOT NULL,
	[FavoriteColor] [varchar](400) NOT NULL
);