CREATE TABLE IF NOT EXISTS `sys`.`Employee`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`FirstName` varchar(400) NOT NULL,
	`LastName` varchar(400) NOT NULL,
	`DOB` DateTime NOT NULL,
	`CreatedAt` DateTime NOT NULL,
	`FavoriteColor` varchar(400) NOT NULL
);
CREATE TABLE IF NOT EXISTS `sys`.`Employee2`(
	`IdentityField` int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`EmployeeListAsJson` varchar(400)  NULL,
	`EmployeeAsJson` varchar(400)  NULL,
	`EmployeeListAsCsv` varchar(400)  NULL,
	`EmployeeAsCsv` varchar(400)  NULL,
	`EmployeeListAsXml` varchar(400)  NULL,
	`EmployeeAsXml` varchar(400)  NULL
);

