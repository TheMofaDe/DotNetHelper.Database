## Bulk Copy


 
### SQL Server Implementation

#### asynchronous 
~~~csharp
var numberOfRowCopied = dbAccess.SqlServerBulkCopyAsync(listofObjects, SqlBulkCopyOptions.Default);
~~~  
#### synchronous
~~~csharp
var numberOfRowCopied = dbAccess.SqlServerBulkCopy(listofObjects, SqlBulkCopyOptions.Default);
~~~  

<br/>

### MySql Implementation

Currently Bulk Copy is only support by SqlServer with this library. If you wish to use bulk copy
with mysql or sqlite your need to implement your own solution. here the officials docs on how to do this
https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html


### Sqlite Implementation

Currently Bulk Copy is only support by SqlServer with this library. If you wish to use bulk copy
with mysql or sqlite your need to implement your own solution.