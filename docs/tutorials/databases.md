
## Switching Database Implementation

So lets go over the senario that the business is moving toward open source only mindset and now all of our
applications need to switch from using a sqlserver database to mysql here how easy it is to switch



## Our current code
~~~csharp
 var dbAccess = new DatabaseAccess<SqlConnection>("ConnectionString");
~~~


## New Code
  * Step #1 reference the MySql.Data nuget package
  * Step #2 now update your code to look like this 
~~~csharp
 var dbAccess = new DatabaseAccess<MySqlConnection>("ConnectionString");
~~~

Yup all it took was reference a new nuget package and changing DBConnection Type and obviously the connections tring 
