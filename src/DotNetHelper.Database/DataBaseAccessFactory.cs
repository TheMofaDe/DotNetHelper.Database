﻿
//#if SUPPORTDBFACTORIES
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.Common;
//using System.Data.SqlClient;
//using System.Globalization;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//using DotNetHelper.Database.Extension;
//using DotNetHelper.Database.Helper;
//using DotNetHelper.ObjectToSql.Enum;
//using DotNetHelper.ObjectToSql.Model;

//namespace DotNetHelper.Database
//{
//    /// <summary>
//    /// A powerful & simple class for dealing CRUD operation that doesn't required you to write sql unless you prefer to do so or need to do something advance
//    /// </summary>
//    public class DatabaseAccessFactory 
//    {
//        /// <summary>
//        /// The connection string to the database
//        /// </summary>
//        public string ConnectionString { get; }
//        /// <summary>
//        /// The time in seconds to wait for the command to execute. The default is 30 seconds.
//        /// </summary>
//        public TimeSpan CommandTimeOut { get; set; } = TimeSpan.FromSeconds(30);
//        /// <summary>
//        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
//        /// </summary>
//       // public TimeSpan ConnectionTimeOut { get; set; } = TimeSpan.FromSeconds(15);
//        /// <summary>
//        /// The service that is used to generate sql
//        /// </summary>
//        public ObjectToSql.Services.ObjectToSql ObjectToSql { get; }
//        /// <summary>
//        /// The type of database. This property is only used to control how sql is generated
//        /// </summary>
//        public DataBaseType DatabaseType => ObjectToSql.DatabaseType;

//        public DbProviderFactory DbProviderFactory { get; private set; }
       
//      //  private object Lock { get; } = new object();
//        public DatabaseAccessFactory(DbProviderFactory dbProviderFactory, DataBaseType type, TimeSpan? commandTimeOut = null)
//        {
//            CommandTimeOut = commandTimeOut.GetValueOrDefault(CommandTimeOut);
//          //  ConnectionTimeOut = connectionTimeOut.GetValueOrDefault(ConnectionTimeOut);
//            DbProviderFactory = dbProviderFactory;
//            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
//            ConnectionString = GetConnectionStringByProvider(DBProviderHelper.Map[type]);
//        }

//        public DatabaseAccessFactory(DataBaseType type, string connectionString = null, TimeSpan? commandTimeOut = null)
//        {
//            CommandTimeOut = commandTimeOut.GetValueOrDefault(CommandTimeOut);
//         //  ConnectionTimeOut = connectionTimeOut.GetValueOrDefault(ConnectionTimeOut);
//            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
//            DbProviderFactory = DbProviderFactories.GetFactory(DBProviderHelper.Map[type]);
//            ConnectionString = connectionString ?? GetConnectionStringByProvider(DBProviderHelper.Map[type]);
//        }


//        // Retrieve a connection string by specifying the providerName.
//        // Assumes one connection string per provider in the config file.
//         string GetConnectionStringByProvider(string providerName)
//        {
//            // Return null on failure.
//            string returnValue = null;

//            // Get the collection of connection strings.
//            var settings = ConfigurationManager.ConnectionStrings;

//            // Walk through the collection and return the first 
//            // connection string matching the providerName.
//            if (settings != null)
//            {
//                foreach (ConnectionStringSettings cs in settings)
//                {
//                    if (cs.ProviderName == providerName)
//                        returnValue = cs.ConnectionString;
//                    break;
//                }
//            }
//            return returnValue;
//        }

//        public DbConnection GetNewConnection(bool openConnection)
//        {
//            var connection = DbProviderFactory.CreateConnection();
//            connection.ConnectionString = ConnectionString;
//            if (openConnection)
//                connection.Open();
//            return connection;
//        }

//        /// <summary>
//        /// return a new instance of DBParameter  
//        /// </summary>
//        /// <param name="parameterName"></param>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public DbParameter GetNewParameter(string parameterName, object value)
//        {
//            var parameter = DbProviderFactory.CreateParameter();
//            parameter.ParameterName = parameterName;
//            parameter.Value = value;
//            return parameter;
//        }

//        /// <summary>
//        /// return a new list of DBParameter where the parameters are created from each property name & value  
//        /// </summary>
//        /// <param name="obj">the object to build DbParameters from</param>
//        /// <returns></returns>
//        public List<DbParameter> GetNewParameter<T>(T obj) where T : class
//        {
//            return GetNewParameter(obj, null, null, null);
//        }


//        /// <summary>
//        /// return a new list of DBParameter where the parameters are created from each property name & value  
//        /// </summary>
//        /// <param name="obj">the object to build DbParameters from</param>
//        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <returns></returns>
//        public List<DbParameter> GetNewParameter<T>(T obj, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class
//        {
//            var list = ObjectToSql.BuildDbParameterList(obj, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
//            return list;
//        }


//        /// <summary>
//        /// creates a new dbcommand from the connection
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DbCommand GetNewCommand(DbConnection connection, string sql, CommandType commandType = CommandType.Text, IEnumerable<DbParameter> parameters = null)
//        {
//            var command = connection.CreateCommand();
//            command.CommandText = sql;
//            command.Connection = connection;
//            command.CommandType = commandType;
//            if (parameters != null)
//                command.Parameters.AddRange(parameters);
//            var isValidInt32 = int.TryParse(CommandTimeOut.TotalSeconds.ToString(CultureInfo.CurrentCulture), out var timeoutInSeconds);
//            command.CommandTimeout = isValidInt32 ? timeoutInSeconds : int.MaxValue;
//            return command;
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <returns></returns>
//        public (DbCommand command, DbTransaction transaction) GetNewCommandAndTransaction(DbConnection connection)
//        {
//            var command = connection.CreateCommand();
//            var isValidInt32 = int.TryParse(CommandTimeOut.TotalSeconds.ToString(CultureInfo.CurrentCulture), out var timeoutInSeconds);
//            command.CommandTimeout = isValidInt32 ? timeoutInSeconds : int.MaxValue;
//            var transaction = connection.BeginTransaction();
//            command.Transaction = transaction;
//            return (command, transaction);

//        }



//        /// <summary>
//        /// Execute an SQL Command and returns the number of rows affected
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        /// See <see cref="ExecuteNonQuery(TDbConnection , string , CommandType , IEnumerable&lt;DbParameter&gt;)"/> to perform this this action with a specified connection.
//        /// <exception cref="System.InvalidOperationException"> </exception>
//        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<DbParameter> parameters = null)
//        {
//            using (var connection = GetNewConnection(true))
//            {
//                var command = GetNewCommand(connection, sql, commandType, parameters);
//                return command.ExecuteNonQuery();
//            }
//        }

//        /// <summary>
//        /// Execute an SQL Command against the specified <typeparamref name="TDbConnection"/>  and returns the number of rows affected
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public int ExecuteNonQuery(DbConnection connection, string sql, CommandType commandType, IEnumerable<DbParameter> parameters = null)
//        {
//            var command = GetNewCommand(connection, sql, commandType, parameters);
//            return command.ExecuteNonQuery();

//        }

//        /// <summary>
//        /// Executes the sql and return the 1st column of the 1st row as an object
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public object ExecuteScalar(string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            using (var connection = GetNewConnection(true))
//            {
//                return ExecuteScalar(connection, sql, commandType, parameters);
//            }
//        }

//        /// <summary>
//        /// Executes the sql and return the 1st column of the 1st row as an object
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public object ExecuteScalar(DbConnection connection, string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var command = GetNewCommand(connection, sql, commandType, parameters);
//            return command.ExecuteScalar();

//        }


//        /// <summary>
//        /// Executes a list of sql as in a single transaction 
//        /// </summary>
//        /// <param name="sqls"></param>
//        /// <param name="rollbackOnException"></param>
//        /// <param name="throwException"></param>
//        /// <returns></returns>
//        public int ExecuteTransaction(List<string> sqls, bool rollbackOnException, bool throwException = true)
//        {
//            var kvps = sqls.Select(s => new KeyValuePair<string, IEnumerable<DbParameter>>(s, null)).AsList();
//            return ExecuteTransaction(kvps, rollbackOnException, throwException);
//        }


//        /// <summary>
//        /// Executes a list of sql as in a single transaction 
//        /// </summary>
//        /// <param name="sqls"></param>
//        /// <param name="rollbackOnException"></param>
//        /// <param name="throwException"></param>
//        /// <returns></returns>
//        public int ExecuteTransaction(List<KeyValuePair<string, IEnumerable<DbParameter>>> sqls, bool rollbackOnException, bool throwException = true)
//        {
//            using (var connection = GetNewConnection(true))
//            {
//                var recordAffected = 0;
//                if (sqls == null || !sqls.Any()) return recordAffected;

//                var obj = GetNewCommandAndTransaction(connection);
//                var command = obj.command;
//                var transaction = obj.transaction;
//                try
//                {
//                    sqls.ForEach(delegate (KeyValuePair<string, IEnumerable<DbParameter>> pair)
//                    {
//                        command.CommandText = pair.Key;
//                        if (pair.Value != null)
//                        {
//                            command?.Parameters?.Clear(); // Clear any previous parameters
//                            command.Parameters.AddRange(pair.Value);
//                        }
//                        recordAffected += command.ExecuteNonQuery();

//                    });
//                    transaction.Commit();
//                }
//                catch (Exception)
//                {
//                    if (rollbackOnException)
//                    {
//                        transaction.Rollback();
//                    }
//                    else
//                    {
//                        transaction.Commit();
//                    }
//                    if (throwException)
//                    {
//                        throw;
//                    }
//                }
//                return recordAffected;
//            }
//        }

       
//        /// <summary>
//        /// execute the sql and return the result as a DbDataReader
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DbDataReader GetDataReader(DbConnection connection, string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var command = GetNewCommand(connection, sql, commandType, parameters);
//            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
//            return reader;
//        }


//        /// <summary>
//        /// execute the sql and return the result as a DbDataReader
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DbDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var connection = GetNewConnection(true);
//            return GetDataReader(connection, sql, commandType, parameters);
//        }

//        /// <summary>
//        /// execute the sql and load the results into a dataTable
//        /// </summary>
//        /// <param name="selectSql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DataTable GetDataTable(string selectSql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var reader = GetDataReader(selectSql, commandType, parameters);
//            var dt = new DataTable();
//            dt.Load(reader);
//            return dt;
//        }

//        /// <summary>
//        /// execute the sql and load the results into a dataTable
//        /// </summary>
//        /// <param name="selectSql"></param>
//        /// <returns></returns>
//        public DataTable GetDataTable(string selectSql, List<DbParameter> parameters = null)
//        {
//            return GetDataTable(selectSql, CommandType.Text,parameters);
//        }

//        /// <summary>
//        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DataTable GetDataTableWithSchema(string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var dt = new DataTable();
//            using (var connection = GetNewConnection(true))
//            {
//                var command = GetNewCommand(connection, sql, commandType, parameters);
//                var schema = command.ExecuteReader(CommandBehavior.SchemaOnly);
//                dt.Load(schema);
//                var data = command.ExecuteReader();
//                dt.Load(data);
//                return dt;
//            }
//        }

//        /// <summary>
//        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
//        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better performance
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public DataTable GetDataTableWithKeyInfo(string sql, CommandType commandType, List<DbParameter> parameters = null)
//        {
//            var dt = new DataTable();
//            using (var connection = GetNewConnection(true))
//            {
//                var command = GetNewCommand(connection, sql, commandType, parameters);
//                var schema = command.ExecuteReader(CommandBehavior.KeyInfo);
//                dt.Load(schema);
//                var data = command.ExecuteReader();
//                dt.Load(data);
//                return dt;
//            }
//        }
//        /// <summary>
//        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public DataTable GetDataTableWithSchema(string sql)
//        {
//            return GetDataTableWithSchema(sql, CommandType.Text);
//        }
//        /// <summary>
//        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
//        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better proformance
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public DataTable GetDataTableWithKeyInfo(string sql)
//        {
//            return GetDataTableWithKeyInfo(sql, CommandType.Text);
//        }


//        /// <summary>
//        /// Attempts to open a connection to the database using the connection string provided in the constructor. 
//        /// </summary>
//        /// <returns></returns>
//        public bool CanConnect()
//        {
//            try
//            {
//                using (GetNewConnection(true))
//                {
//                    return true;
//                }
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }

//        /// <summary>
//        /// Executes the specified sql and maps the results a list of objects
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="xmlDeserializer"></param>
//        /// <param name="jsonDeserializer"></param>
//        /// <param name="csvDeserializer"></param>
//        /// <param name="parameters"></param>
//        /// <returns>The sql result mapped to a list of </returns>
//        public List<T> Get<T>(string sql, CommandType commandType, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class
//        {
//            return GetDataReader(sql, commandType, parameters).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
//        }

//        /// <summary>
//        /// Executes the specified sql and maps the results a list of objects
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="connection"></param>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="xmlDeserializer"></param>
//        /// <param name="jsonDeserializer"></param>
//        /// <param name="csvDeserializer"></param>
//        /// <param name="parameters"></param>
//        /// <returns>The sql result mapped to a list of </returns>
//        public List<T> Get<T>(DbConnection connection, string sql, CommandType commandType, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class
//        {
//            return GetDataReader(connection, sql, commandType, parameters).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
//        }

//        /// <summary>
//        /// return a list of type of T from the database. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <returns></returns>
//        public List<T> Get<T>() where T : class
//        {
//            return Get<T>(null, null, null);
//        }



//        /// <summary>
//        /// return a list of type of T from the database. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="xmlDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <returns></returns>
//        public List<T> Get<T>(Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
//        {
//            var sqlTable = new SqlTable(DatabaseType, typeof(T));
//            return Get<T>($"SELECT * FROM {sqlTable.FullNameWithBrackets}", CommandType.Text, xmlDeserializer, jsonDeserializer, csvDeserializer);
//        }

//        /// <summary>
//        /// Executes the sql and map the results to a list of type of T
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="sql"></param>
//        /// <param name="commandType">Specifies how a command string is interpreted.</param>
//        /// <param name="parameters"></param>
//        /// <returns></returns>
//        public List<T> Get<T>(string sql, CommandType commandType, List<DbParameter> parameters = null) where T : class
//        {
//            return Get<T>(sql, commandType, null, null, null, parameters);
//        }

//        /// <summary>
//        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from</param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <returns># of rows affected</returns>
//        public int Execute<T>(T instance, ActionType actionType) where T : class
//        {
//            return Execute(instance, actionType, null, null, null, null);
//        }

//        /// <summary>
//        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="tableName">Table name to use when generating sql </param>
//        /// <returns></returns>
//        public int Execute<T>(T instance, ActionType actionType, string tableName) where T : class
//        {
//            return Execute(instance, actionType, tableName, null, null, null);
//        }

//        /// <summary>
//        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="tableName">Table name to use when generating sql </param>
//        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <returns></returns>
//        public int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class
//        {
//            if (instance.GetType().IsTypeAnIEnumerable()) // getTYPE is faster than TYPEOF(T)
//            {
//                if (instance is IEnumerable<object> list)
//                {
//                    var sqls = new List<KeyValuePair<string, IEnumerable<DbParameter>>>() { };

//                    foreach (var item in list)
//                    {
//                        var sql = ObjectToSql.BuildQuery(actionType, item, tableName);
//                        var paramters = GetNewParameter(item);
//                        sqls.Add(new KeyValuePair<string, IEnumerable<DbParameter>>(sql,paramters));
//                    }
//                    return ExecuteTransaction(sqls, true, true);
//                }
//                throw new InvalidOperationException($"The type {typeof(T)} is not supported");
//            }
//            else
//            {
//                var sql = ObjectToSql.BuildQuery(actionType, instance, tableName);
//                var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
//                return ExecuteNonQuery(sql, CommandType.Text, parameters);
//            }
//        }




//        /// <summary>
//        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="tableName">Table name to use when generating sql </param>
//        /// <param name="keyFields"></param>
//        /// <returns></returns>
//        public int Execute<T>(T instance, ActionType actionType, string tableName, params Expression<Func<T, object>>[] keyFields) where T : class
//        {
//            var sql = ObjectToSql.BuildQuery(actionType, tableName, keyFields);
//            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, null, null, null);
//            return ExecuteNonQuery(sql, CommandType.Text, parameters);
//        }


//        /// <summary>
//        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="tableName">Table name to use when generating sql </param>
//        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <param name="keyFields">Override attributes and specified which properties are keys from an expression</param>
//        /// <returns></returns>
//        public int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer, params Expression<Func<T, object>>[] keyFields) where T : class
//        {
//            var sql = ObjectToSql.BuildQuery(actionType, tableName, keyFields);
//            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
//            return ExecuteNonQuery(sql, CommandType.Text, parameters);
//        }


//        /// <summary>
//        ///  Creates the specified SQL from the object then executes the sql and applies the reflected values to the instance provided. This is useful when dealing with identity fields  
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="outputFields">the fields to return that will reflect the values after the sql is executed  </param>
//        /// <returns></returns>
//        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
//        {
//            return ExecuteAndGetOutput(instance, actionType, null, null, null, null, null, null, outputFields);
//        }

//        /// <summary>
//        ///  Creates the sql and applies the reflected values tothe specified SQL from the object then executes t the instance provided. This is useful when dealing with identity fields  
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="instance">the object to create sql from </param>
//        /// <param name="actionType">type of sql to generate</param>
//        /// <param name="xmlDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
//        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
//        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
//        /// <param name="outputFields"></param>
//        /// <returns></returns>
//        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType
//            , Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer
//            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
//            , params Expression<Func<T, object>>[] outputFields) where T : class
//        {
           
//            var sql = ObjectToSql.BuildQueryWithOutputs(actionType,null, outputFields);
//            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
//            return GetDataReader(sql, CommandType.Text, parameters).MapTo<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
//        }


//        public DbDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
//        {
//            return ExecuteAndGetOutputAsDataReader(instance, actionType, null, null, null, outputFields);
//        }

//        public DbDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType
//            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
//            , params Expression<Func<T, object>>[] outputFields) where T : class
//        {
           
//            var sql = ObjectToSql.BuildQueryWithOutputs(actionType,null, outputFields);
//            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
//            return GetDataReader(sql, CommandType.Text, parameters);
//        }



//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <returns># of records inserted</returns>
//        public long SqlServerBulkCopy<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions) where T : class
//        {
//            return SqlServerBulkCopy(data, bulkCopyOptions, null);
//        }


//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <param name="tableName">table name to insert data into</param>
//        /// <returns># of records inserted</returns>
//        public long SqlServerBulkCopy<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions, string tableName) where T : class
//        {
//            return SqlServerBulkCopy(data, bulkCopyOptions, tableName, 0);
//        }

//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <param name="tableName">table name to insert data into</param>
//        /// <param name="batchSize">The integer value of the BatchSize property, or zero if no value has been set.</param>
//        /// <returns># of records inserted</returns>
//        public long SqlServerBulkCopy<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions, string tableName, int batchSize) where T : class
//        {
//            if (DatabaseType != DataBaseType.SqlServer)
//                throw new InvalidOperationException($"This library doesn't reference a {DatabaseType}BulkCopy so its not supported.");
//            var dt = data.MapToDataTable(tableName);
//            using (var bulk = new SqlBulkCopy(ConnectionString, bulkCopyOptions))
//            {
//                long rowsCopied = 0;
//                bulk.DestinationTableName = dt.TableName;
//                foreach (DataColumn dc in dt.Columns)
//                {
//                    bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
//                }
//                bulk.BatchSize = batchSize;
//                bulk.NotifyAfter = dt.Rows.Count;
//                bulk.SqlRowsCopied += (s, e) => rowsCopied = e.RowsCopied;
//                bulk.WriteToServer(dt);
//                return rowsCopied;
//            }
//        }





//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <returns># of records inserted</returns>
//        public async Task<long> SqlServerBulkCopyAsync<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions) where T : class
//        {
//            return await SqlServerBulkCopyAsync(data, bulkCopyOptions, null);
//        }


//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <param name="tableName">table name to insert data into</param>
//        /// <returns># of records inserted</returns>
//        public async Task<long> SqlServerBulkCopyAsync<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions, string tableName) where T : class
//        {
//            return await SqlServerBulkCopyAsync(data, bulkCopyOptions, tableName, 0);
//        }

//        /// <summary>
//        /// Perform a SQLBulkCopy 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="data"></param>
//        /// <param name="bulkCopyOptions">bulk copy option</param>
//        /// <param name="tableName">table name to insert data into</param>
//        /// <param name="batchSize">The integer value of the BatchSize property, or zero if no value has been set.</param>
//        /// <returns># of records inserted</returns>
//        public async Task<long> SqlServerBulkCopyAsync<T>(List<T> data, SqlBulkCopyOptions bulkCopyOptions, string tableName, int batchSize) where T : class
//        {
//            if (DatabaseType != DataBaseType.SqlServer)
//                throw new InvalidOperationException($"This library doesn't reference a {DatabaseType}BulkCopy so its not supported.");
//            var dt = data.MapToDataTable(tableName);
//            using (var bulk = new SqlBulkCopy(ConnectionString, bulkCopyOptions))
//            {
//                long rowsCopied = 0;
//                bulk.DestinationTableName = dt.TableName;
//                foreach (DataColumn dc in dt.Columns)
//                {
//                    bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
//                }
//                bulk.BatchSize = batchSize;
//                bulk.NotifyAfter = dt.Rows.Count;
//                bulk.SqlRowsCopied += (s, e) => rowsCopied = e.RowsCopied;
//                await bulk.WriteToServerAsync(dt);
//                return rowsCopied;
//            }
//        }


//        // Flag: Has Dispose already been called?
//        private bool _disposed;

//        // Public implementation of Dispose pattern callable by consumers.
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        // Protected implementation of Dispose pattern.
//        protected virtual void Dispose(bool disposing)
//        {
//            if (_disposed)
//                return;

//            if (disposing)
//            {
//                // Free any other managed objects here.
//            }

//            // Free any unmanaged objects here.
//            //
//            _disposed = true;
//        }
//    }
//}
//#endif