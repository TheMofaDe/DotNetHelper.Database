## CRUD Operations

This article will explain everyting related to inserting objects into a database using this library.


*Lets go over some prerequisites 

*Employee Table In Database*
~~~sql
CREATE TABLE [master].[dbo].[Employee](
	[IdentityField] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
	[FirstName] [varchar](400) NOT NULL,
	[LastName] [varchar](400) NOT NULL,
	[DOB] DateTime NOT NULL,
	[CreatedAt] DateTime NULL DEFAULT GETDATE(),
	[FavoriteColor] [varchar](400)  NULL
);
~~~

*Employee Class In .NET Project*
~~~csharp
 public class Employee {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tells the library this column is a identity column
        [Key] // Tells the library this column is a primary key
        public int IdentityField { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FavoriteColor { get; set; }
        public DateTime? CreatedAt { get; set; }

        [NotMapped]// Tells the library to ignore this property its not part of the database
        public string FullName => FirstName + " " + LastName;

        [SqlColumn(MapTo = "DOB")] // Tells the library this property is actually name DOB in the database table
        public DateTime DateOfBirth { get; set; }
}
~~~


## Getting Started With Inserts

Now I will show you how to insert an a employee object into your database 


The first thing you will need to do is intialize a new instance of **DatabaseAccess**   
To initialize a new instance of **DatabaseAccess** you must specify a type of DBConnection your database support.   

~~~csharp 
var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
~~~

For this example im going to use the common *SqlConnection* type as showned above.    
If your wondering why this is required. It is because this library supports multiple database 
and not every database use the same syntax. So the DBConnection that you use will control how sql is generated. 

~~~csharp
var employee = new Employee(); // create an new employee
    employee.FirstName = "John";
    employee.LastName = "Doe";
    employee.DateOfBirth = DateTime.Now;
               
var recordAffected = dbAccess.Execute(employee, ActionType.Insert); // ActionType is a enum of Insert,Update,Delete,Upsert
~~~


**That was simply and east**

<br/>


## Getting Started With Deletes

This section assumes you read the above section. Now that we added an employee we need to remove him because we don't like him
here how you would go about doing such a thing 

~~~csharp 
 var employees = dbAccess.Get<Employee>(); // returns a list of T which in this cause is a list of Employee
~~~

Now our employees object contains one employee in it. Lets delete him   
~~~csharp 
var recordAffected = dbAccess.Execute(employees.First(), ActionType.Delete); 
~~~ 

**That was simply and east**

<br/>



## Getting Started With Upserts

This section assumes you read the above section.    
Oops maybe deleting that employee was a mistake lets we add him using an upsert
  
~~~csharp 
var recordAffected = dbAccess.Execute(employee, ActionType.Upsert); 
~~~ 

**That was simply and east**


## Getting Started With Update

This section assumes you read the above section.    
Turns out our test employee John real name isn't John its Patrick so lets update him
  
~~~csharp 
employee.FirstName = "Patrick";
var recordAffected = dbAccess.Execute(employee, ActionType.Update); 
~~~ 

**That was simply and east**