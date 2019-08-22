# DotNetHelper.Database


#### *DotNetHelper.Database is a lightweight ORM that works with generics,dynamic, & anonyous objects for ADO.NET * 

|| [**Documentation**][Docs] • [**API**][Docs-API] • [**Tutorials**][Docs-Tutorials] ||  [**Change Log**][Changelogs] • || [**View on Github**][Github]|| 

| Package  | Tests | Code Coverage |
| :-----:  | :---: | :------: |
| ![Build Status][nuget-downloads]  | ![Build Status][tests]  | [![codecov](https://codecov.io/gh/TheMofaDe/DotNetHelper.Database/branch/master/graph/badge.svg)](https://codecov.io/gh/TheMofaDe/DotNetHelper.Database) |


| Continous Integration | Windows | Linux | MacOS | 
| :-----: | :-----: | :-----: | :-----: |
| **AppVeyor** | [![Build status](https://ci.appveyor.com/api/projects/status/9mog32m4mejqyd3i?svg=true)](https://ci.appveyor.com/project/TheMofaDe/dotnethelper-database)  | | |
| **Azure Devops** | ![Build Status][azure-windows]  | ![Build Status][azure-linux]  | ![Build Status][azure-macOS] | 




## Features
+ Can dynamically build your sql statement from any *(Generic,Anonymous,Dynamic)* object but also support you providing one as well
+ Can  *(Insert,Update,Upsert,Delete)* any *(Generic,Anonymous,Dynamic)* object into database
+ Support Any **IDbConnection** & work with **DbFactory**
+ Support auto-serializing & deserializing columns that is as stored as CSV,JSON, & XML in the database 
  + You implement the serialization so we don't have to depend on libraries like newtonsoft.json 
+ Map List To **DataTable**
+ Map **DataTable** To List
+ Map **IDataReader** To List
+ Map **DataRow** To A Class

## Example 

##### For this example my table in the database will look like this
```sql 
CREATE TABLE [master].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NOT NULL DEFAULT GETDATE(),
	[FavoriteColor] [varchar](400)  NULL
);
```

##### My employee class will look like this 
```csharp
public class Employee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Marks this property as an identity field 
        [Key]  // marks this property as a primary key... 
        public int IdentityField { get; set; }
        [NotMapped] // This property will be ignore when performing database actions
        public string FullName => FirstName + " " + LastName;
        [SqlColumn(MapTo = "DOB")]  // This property is actually name DOB in the database
        public DateTime DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FavoriteColor { get; set; }
        public DateTime CreatedAt { get; } = new DateTime();
    }
```




##### How to insert an employee using a generic class
```csharp
 var employee = new Employee() {
  DateOfBirth = DateTime.Today
, FavoriteColor = "Blue"
, FirstName = "Joe" 
, LastName = "Generic"
}; 
 var dbAccess = new DatabaseAccess<SqlConnection, SqlParameter>(DataBaseType.SqlServer, "ConnectionString");
 var recordAffected = dbAccess.Execute(employee, ActionType.Insert); // ActionType is a enum of Insert,Update,Delete,Upsert
```   
##### How to insert an employee using a dynamic object
```csharp
dynamic dynamicEmployee = new ExpandoObject(); // A DYNAMIC EMPLOYEE
dynamicEmployee.FirstName = "Joe Sister";
dynamicEmployee.LastName = "Dynamic";
dynamicEmployee.DOB = DateTime.Today.AddDays(-2);
```
##### How to insert an employee using a generic class
```csharp
var anonymousEmployee = new {FirstName = "Joe Brother", DOB = DateTime.Today.AddDays(-1) , LastName = "Anonymous"}; // A ANONYMOUS EMPLOYEE
var dbAccess = new DatabaseAccess<SqlConnection, SqlParameter>(DataBaseType.SqlServer, "ConionString"); // Specify database provider to ensure syntax is correct
var recordAffected += dbAccess.Execute(anonymousEmployee, ActionType.Insert,"Employee"); // you need to specify the table name when using anonymous objects
```
     

##### Finish Product 
![alt text][logo]



## Documentation
For more information, please refer to the [Officials Docs][Docs]

<!-- Links. -->
## Solution Template
[![badge](https://img.shields.io/badge/Built%20With-DotNet--Starter--Template-orange.svg)](https://github.com/TheMofaDe/DotNet-Starter-Template)


<!-- Links. -->

[1]:  https://gist.github.com/davidfowl/ed7564297c61fe9ab814

[Cake]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[Azure DevOps]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[AppVeyor]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[GitVersion]: https://gitversion.readthedocs.io/en/latest/
[Nuget]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[Chocolately]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[WiX]: http://wixtoolset.org/
[DocFx]: https://dotnet.github.io/docfx/
[Github]: https://github.com/TheMofaDe/DotNetHelper.Database
[logo]: docs/images/snippet1.gif "Snippet 1"


<!-- Documentation Links. -->
[Docs]: https://themofade.github.io/DotNetHelper.Database/index.html
[Docs-API]: https://themofade.github.io/DotNetHelper.Database/api/DotNetHelper.Database.html
[Docs-Tutorials]: https://themofade.github.io/DotNetHelper.Database/tutorials/index.html
[Docs-samples]: https://dotnet.github.io/docfx/
[Changelogs]: https://dotnet.github.io/docfx/


<!-- BADGES. -->

[nuget-downloads]: https://img.shields.io/nuget/dt/DotNetHelper.Database.svg?style=flat-square
[tests]: https://img.shields.io/appveyor/tests/TheMofaDe/DotNetHelper.Database.svg?style=flat-square
[coverage-status]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Windows

[azure-windows]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Windows
[azure-linux]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Linux
[azure-macOS]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=macOS
[app-veyor]: https://ci.appveyor.com/project/TheMofaDe/dotnethelper-database