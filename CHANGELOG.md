# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

### Unreleased
##


## [1.0.64] - 2020-06-05
### Added
*   Async Support everywhere


### Removed 
*   Remove IDatabaseAccess interface.
<br/>



<br/>


## [1.0.63] - 2019-11-08
### Bug Fixes
*   fix bug  when executing aganist a list of objects using the Execute method wasn't clearing dbparameters 

<br/>


## [1.0.60] - 2019-09-19
### Added
- Add boolean property UseSingleConnection to DataAccess class that will use the same SqlConnection for all execution
~~~csharp
public bool UseSingleConnection { get; set; }
~~~
- Add extension method for DBConnection objects to gain access to a DataAccessClass
~~~csharp
 public static IDatabaseAccess DatabaseAccess<T>(this T connection, DataBaseType? dataBaseType = null) where T : DbConnection, new()
 {
     return new DatabaseAccess<T>(connection,dataBaseType);
 }
~~~ 

### Changed 
*   Renamed method DatabaseAccess.SqlServerBulkInsert to DatabaseAccess.SqlServerBulkCopy
*   Fix bug where MapToList didn't working correctly for dynamic objects

### Removed 
*   Removed paramter ConnectionTimeOut from the DataAccess constructor and interface due to DBConnection.ConnectionTimeOut being a read-only field

<br/>


## [1.0.54] - 2019-08-26
### Added
- SQLServer bulk insert support 
~~~csharp
DatabaseAccess.SqlServerBulkInsert(listOfObjects, SqlBulkCopyOptions.Default);
~~~

### Changed 
*   made connection string property public in IDataBaseAccess
*   added new constructor to databaseeaccess

### Bug Fixes
*   fix bug in maptodatabase while not supported exception was being thrown when dealing with nullable<T> 

<br/>

## [1.0.44] - 2019-08-22
### Changed 
- enchanced support anonymous objects

<br/>

## [1.0.41] - 2019-08-21
### Added 
- New APIs for update,upsert,delete support with anonymous objects
~~~csharp
int Execute<T>(T instance, ActionType actionType, string tableName, params Expression<Func<T, object>>[] keyFields) where T : class;
int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer, params Expression<Func<T, object>>[] keyFields) where T : class;
~~~
<br/>

## [1.0.40] - 2019-08-19
### Added
- Added new api for getting provider name based off **DatabaseType**
see 
~~~csharp
public static class DBProviderHelper
~~~

### Changed
- Fix bug in MapTo where mapping T to string was returning null if datareader value was a type other than string
~~~csharp
public static T MapTo<T>(this IDataReader reader) where T : class
~~~



[1.0.40]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.40
[1.0.41]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.41
[1.0.44]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.44
[1.0.54]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.54
[1.0.60]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v1.0.60


