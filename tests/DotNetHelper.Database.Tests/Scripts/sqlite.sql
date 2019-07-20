﻿CREATE TABLE IF NOT EXISTS [Employee](
	[IdentityField] [INTEGER] NOT NULL PRIMARY KEY AUTOINCREMENT,
	[FirstName] TEXT(400) NOT NULL,
	[LastName] TEXT(400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NOT NULL,
	[FavoriteColor] TEXT(400) NOT NULL
);

