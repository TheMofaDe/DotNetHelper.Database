# DotNetHelper.Database


#### *DotNetHelper.Database is a ORM that support CRUD + upsert operations with any generic,dynamic, & anonyous objects for .NET * 

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
+ Support Any **DbConnection** & work with **DbFactory**
+ Support auto-serializing & deserializing columns that is as stored as CSV,JSON, & XML in the database 
  + You implement the serialization so we don't have to depend on libraries like newtonsoft.json 
+ Map List To **DataTable**
+ Map **DataTable** To List
+ Map **IDataReader** To List
+ Map **DataRow** To A Class

## Supports 
+   SqlServer
+   Sqlite
+   MySql




## Example 

##### How to insert an employee using a generic class
```csharp
 var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
 var recordAffected = dbAccess.Execute(employee, ActionType.Insert); // ActionType is a enum of Insert,Update,Delete,Upsert
```   
##### How to insert an employee using a dynamic object
```csharp
dynamic dyn = new ExpandoObject(); 
        dyn.FirstName = "Joe Sister";
        dyn.LastName = "Dynamic";
        dyn.DOB = DateTime.Today;
var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
var recordAffected = dbAccess.Execute(dyn, ActionType.Insert,"TableName"); // you need to specify the table name when using dynamic objects
```
##### How to insert an employee using a generic class
```csharp
var anonymousEmployee = new {FirstName = "Joe Brother", DOB = DateTime.Today.AddDays(-1) , LastName = "Anonymous"}; 
var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
var recordAffected = dbAccess.Execute(anonymousEmployee, ActionType.Insert,"TableName"); // you need to specify the table name when using anonymous objects
```

##### How to use bulk copy with a list of objects
~~~csharp
var numberOfRowCopied = dbAccess.SqlServerBulkInsert(listofObjects, SqlBulkCopyOptions.Default);```
~~~        
 

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
[Changelogs]: https://github.com/TheMofaDe/DotNetHelper.Database/blob/master/CHANGELOG.md


<!-- BADGES. -->

[nuget-downloads]: https://img.shields.io/nuget/dt/DotNetHelper.Database.svg?style=flat-square
[tests]: https://img.shields.io/appveyor/tests/TheMofaDe/DotNetHelper.Database.svg?style=flat-square
[coverage-status]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Windows

[azure-windows]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Windows
[azure-linux]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=Linux
[azure-macOS]: https://dev.azure.com/Josephmcnealjr0013/DotNetHelper.Database/_apis/build/status/TheMofaDe.DotNetHelper.Database?branchName=master&jobName=macOS
[app-veyor]: https://ci.appveyor.com/project/TheMofaDe/dotnethelper-database