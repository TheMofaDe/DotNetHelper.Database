# DotNetHelper.Database

#### *DotNetHelper.Database takes your generic types or dynamic & anonymous objects and convert it to sql.* 

|| [**View on Github**][Github] || 


## Features
+ INSERT
+ UPDATE
+ DELETE
+ UPSERT
+ INSERT with OUTPUT Columns
+ UPDATE with OUTPUT Columns
+ DELETE with OUTPUT Columns
+ UPSERT with OUTPUT Columns

## Supported Databases
+ SQLSERVER
+ SQLITE
+ MYSQL
+ More to come


## How to use
##### How to Use With Generics Types
```csharp
public class Employee {
      public FirstName { get; set; }
      public LastName  { get; set; }
}
            var sqlServerObjectToSql = new ObjectToSql(DataBaseType.SqlServer);
            var insertSql = sqlServerObjectToSql.BuildQuery<Employee>("TABLE NAME OR DEFAULT TO TYPE NAME", ActionType.Insert);
// OR 
            var insertSql = sqlServerObjectToSql.BuildQuery("TABLE NAME OR DEFAULT TO TYPE NAME", ActionType.Insert,typeof(Employee));
```

##### How to Use With Dynamic Objects
```csharp
            var sqlServerObjectToSql = new ObjectToSql(DataBaseType.SqlServer);
            dynamic record = new ExpandoObject();
            record.FirstName = "John";
            record.LastName = "Doe";
            var insertSql = sqlServerObjectToSql.BuildQuery("TABLE NAME OR DEFAULT TO TYPE NAME", ActionType.Insert,record);
```


##### How to Use With Anonymous Objects
```csharp
            var sqlServerObjectToSql = new ObjectToSql(DataBaseType.SqlServer);
            var anonymousObject = new { FirstName = "John" , LastName = "Doe"}
            var insertSql = sqlServerObjectToSql.BuildQuery("TABLE NAME OR DEFAULT TO TYPE NAME", ActionType.Insert,anonymousObject);
```
##### Output
```sql
INSERT INTO TableNameGoHere ([FirstName],[LastName]) VALUES (@FirstName,@LastName)
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


<!-- Documentation Links. -->
[Docs]: https://themofade.github.io/DotNetHelper.Database/index.html
[Docs-API]: https://themofade.github.io/DotNetHelper.Database/api/DotNetHelper.Database.Attribute.html
[Docs-Tutorials]: https://themofade.github.io/DotNetHelper.Database/tutorials/index.html
[Docs-samples]: https://dotnet.github.io/docfx/
[Changelogs]: https://dotnet.github.io/docfx/