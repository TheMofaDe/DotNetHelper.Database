# DotNetHelper.Database

#### *DotNetHelper.Database is a lightweight ORM that works with generics,dynamic, & anonyous objects for ADO.NET * 

|| [**View on Github**][Github] || 


## Features
+ Can dynamically build your sql statement from any *(Generic,Anonymous,Dynamic)* object but also support you providing on as well
+ Can  *(Insert,Update,Upsert,Delete)* any *(Generic,Anonymous,Dynamic)* object into database
+ Support Any **IDbConnection** & work with **DbFactory**
+ Support auto-serializing & deserializing columns that is as stored as CSV,JSON, & XML in the database 
  + You implement the serialization so we don't have to depend on libraries like newtonsoft.json 
+ Map List To **DataTable**
+ Map **DataTable** To List
+ Map **IDataReader** To List
+ Map **DataRow** To A Class


## Supported Databases
+ SQLSERVER
+ SQLITE
+ More to come


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

##### My generic object class will look like this 
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

##### Now lets add some new employees from code using dynamic,anonymous & generic objects



```csharp
 dynamic dynamicEmployee = new ExpandoObject(); // A DYNAMIC EMPLOYEE
                    dynamicEmployee.FirstName = "Joe Sister";
                    dynamicEmployee.LastName = "Dynamic";
                    dynamicEmployee.DOB = DateTime.Today.AddDays(-2);

            var employee = new Employee() {DateOfBirth = DateTime.Today, FavoriteColor = "Blue", FirstName = "Joe" , LastName = "Generic"}; // A GENERIC EMPLOYEE
            var anonymousEmployee = new {FirstName = "Joe Brother", DOB = DateTime.Today.AddDays(-1) , LastName = "Anonymous"}; // A ANONYMOUS EMPLOYEE

            var dbAccess = new DatabaseAccess<SqlConnection, SqlParameter>(DataBaseType.SqlServer, "Server=localhost;Initial Catalog=master;Integrated Security=True"); // Specify database provider to ensure syntax is correct

            // Lets add our 3 employees to the database note 
            var recordAffected = dbAccess.Execute(employee, ActionType.Insert); // ActionType is a enum of Insert,Update,Delete,Upsert
            recordAffected += dbAccess.Execute(anonymousEmployee, ActionType.Insert,"Employee"); // you need to specify the table name when using anonymous objects
            recordAffected += dbAccess.Execute<ExpandoObject>(dynamicEmployee, ActionType.Insert,"Employee");  // you need to specify the table name when using dynamic objects

```

##### Finish Product 
![alt text][logo]


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
[Changelogs]: https://dotnet.github.io/docfx/