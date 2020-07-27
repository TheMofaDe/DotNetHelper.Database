# DotNetHelper.Database


#### *DotNetHelper.Database is a ORM that support CRUD + upsert operations with any generic,dynamic, & anonyous objects for .NET* 


|| [**View on Github**][Github] ||



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



## Getting Started 

##### insert,update,upsert,delete using a anonymous object 
```csharp
using (var db = new SqlConnection().DB()) 
{
      // Same api can be used for Update,Upsert,Delete by changing the ActionType enum 
      db.Execute(new {Id = 1, Name = "John Doe"}, ActionType.Insert,"TableName"); 
}
```   
##### insert,update,upsert,delete using a dynamic object
```csharp
dynamic dyn = new ExpandoObject(); 
        dyn.Id = 2;
        dyn.Name = "John Sister";
using (var db = new SqlConnection().DB()) 
{
   // Same api can be used for Update,Upsert,Delete by changing the ActionType enum
      db.Execute(dyn, ActionType.Insert,"TableName"); 
}
```
##### insert,update,upsert,delete using a generic class
```csharp
using (var db = new SqlConnection().DB()) 
{
      var employee = new Employee(){ Id = 1, Name = "Generic Name" }  
   // Same api can be used for Update,Upsert,Delete by changing the ActionType enum
      db.Execute(employee, ActionType.Insert); 
   // table name not required if Table Attribute exist otherwise type name would be used as table name
}
```

##  
### Insert record and return database generated values such as Identity & more

##### Lets create a table
~~~sql 
CREATE TABLE [Employee](
	[Id] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[Name] [varchar](400) NOT NULL,
	[CreatedAt] DateTime NOT NULL DEFAULT GETDATE()
);
~~~


##### Lets create our DTO from our table
```csharp
public class Employee {
  
  [SqlColumn(SetIsIdentityKey = true)] // Specify that this column is an Identity field
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // or you can use data annotation attribute for same behavior
  public int Id { get; set; }

  public string Name { get; set; }
  
  [SqlColumn(SetIsReadOnly = true)] // If true this property will never be included when creating insert sql. This is useful for senarios where you want to use the database default value
  [NotMapped] // or you can use data annotation attribute for same behavior
  public DateTime CreatedAt { get; set; }

}
```
##### now lets insert an employee and get back all the database generated values such as CreatedAt & Identity  Id
```csharp
using (var db = new SqlConnection().DB()) 
{
  var employee = new Employee() {Name = "Generic Name"};
  var outputEmployee = await db.ExecuteAndGetOutputAsync(employee, ActionType.Insert,emp => emp.Id, emp => emp.CreatedAt);
  // Set the identity and database default value back to my original unchanged object
  employee.Id = outputEmployee.Id;
  employee.CreatedAt = outputEmployee.CreatedAt;

  Console.WriteLine(employee.Id); // 1
  Console.WriteLine(employee.CreatedAt); // 7/24/2020 7:55:45 PM
  Console.WriteLine(employee.Name); // Generic Name
}
```

<!-- Links. -->

[1]:  https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[2]: http://themofade.github.io/DotNetHelper.Database

[Cake]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[Azure DevOps]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[AppVeyor]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[GitVersion]: https://gitversion.readthedocs.io/en/latest/
[Nuget]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[Chocolately]: https://gist.github.com/davidfowl/ed7564297c61fe9ab814
[WiX]: http://wixtoolset.org/
[DocFx]: https://dotnet.github.io/docfx/
[Github]: https://github.com/TheMofaDe/DotNetHelper.Database
[logo]: images/snippet1.gif "Snippet 1"

<!-- Documentation Links. -->
[Docs]: https://themofade.github.io/DotNetHelper.Database/index.html
[Docs-API]: https://themofade.github.io/DotNetHelper.Database/api/DotNetHelper.Database.Attribute.html
[Docs-Tutorials]: https://themofade.github.io/DotNetHelper.Database/tutorials/index.html
[Docs-samples]: https://dotnet.github.io/docfx/
[Changelogs]: https://github.com/TheMofaDe/DotNetHelper.Database/blob/master/CHANGELOG.md