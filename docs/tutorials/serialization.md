## Working with CSV,JSON,XML Columns

Sometimes we find ourselves storing xml,json, or csv values in our database table. 
Whether your doing it to be lazy or doing it for a good cause. This libraries supports it


*Role Class In .NET Project*
~~~csharp
public class Role {
    public int RoleId {get;set;}
    public string Role {get;set;}
}
~~~
*Employee Class In .NET Project*
~~~csharp
public class Employee {
     
    [SqlColumn(SerializableType = SerializableType.Json)] 
    public List<Role> Roles { get; set; }
    public string FirstName { get; set;}
    public string LastName { get; set; }
    public int Id { get; set; }
}
~~~

You can assign an attribute to a property to indicate that it need to be serialize before inserting into the database and deserialize to a class when mapping to an object. This is done in the above snippet using 
~~~csharp
 [SqlColumn(SerializableType = SerializableType.Json)] 
~~~


#### What if I need to customize how serialization & deserialization works?

This library allows you to implement your own serialization & deserialization    
by doing so we won't need to depend on libraries like Newtonsoft.Json, CSVHelper. This 
is a win because now your not force to use a specific version or limited on when you can upgrade version.

#### Lets Review how to do this
 

*Api Definition in DatabaseAccess Class*
~~~csharp
int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class;
~~~

Lets create a new instance of our employee class above

~~~csharp
var employee = new Employee();
~~~
Now we need an instance of DatabaseAccess
~~~csharp
var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
~~~
Now we can insert the employee and its property **Roles** will be inserted in our employee table in a the column Roles    
but its value will be in Json format
~~~csharp
 dbAccess.Execute(employee, ActionType.Insert, null,o => JsonConvert.SerializeObject(o),null,null );
~~~
           
There you have it.