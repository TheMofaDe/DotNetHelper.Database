﻿CREATE TABLE [Test].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1),
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DateOfBirth] DateTime NOT NULL,
	[CreatedAt] DateTime NOT NULL,
	[FavoriteColor] [varchar](400) NOT NULL
)