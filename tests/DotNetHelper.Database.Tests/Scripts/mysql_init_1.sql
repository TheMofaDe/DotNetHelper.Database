CREATE TABLE IF NOT EXISTS `sys`.`Employee`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`FirstName` varchar(400) NOT NULL,
	`LastName` varchar(400) NOT NULL,
	`DOB` DateTime NOT NULL,
	`CreatedAt` DateTime NULL,
	`FavoriteColor` varchar(400) NOT NULL
);
CREATE TABLE IF NOT EXISTS `sys`.`Employee2`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`EmployeeListAsJson` varchar(800)  NULL,
	`EmployeeAsJson` varchar(800)  NULL,
	`EmployeeListAsCsv` varchar(800)  NULL,
	`EmployeeAsCsv` varchar(800)  NULL,
	`EmployeeListAsXml` varchar(800)  NULL,
	`EmployeeAsXml` varchar(800)  NULL
);

