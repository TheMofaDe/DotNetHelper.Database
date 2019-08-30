
## IDataReader Extensions




#### Map To List
This extension will take any datareader and map it to a list of any class

~~~csharp
// DEFINITION
 public static List<T> MapToList<T>(this IDataReader reader) where T : class

// EXAMPLE USAGE
IDataReader dataReader;
dataReader.MapToList<dynamic>();  // Works with dynamic
dataReader.MapToList<Employee>(); // Most common use would be using strongly typed class
~~~


####