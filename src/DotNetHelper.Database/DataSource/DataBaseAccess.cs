using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using DotNetHelper.Database.Extension;
using DotNetHelper.Database.Interface;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Model;

namespace DotNetHelper.Database.DataSource
{
    /// <summary>
    /// A powerful & simple class for dealing with simple CRUD operation that doesn't required you to write sql but also provided an overload to pass sql if needed
    /// </summary>
    /// <typeparam name="C">An implementation of IDBConnection</typeparam>
    /// <typeparam name="P">The corresponding DbParameter class to ties with the specified IDBConnection </typeparam>
    public class DatabaseAccess<C, P> : IDatabaseAccess where C : class, IDbConnection, IDisposable, new() where P : DbParameter, new()
    {
        /// <summary>
        /// The connection string to the database
        /// </summary>
        private string ConnectionString { get; }
        /// <summary>
        /// The time in seconds to wait for the command to execute. The default is 30 seconds.
        /// </summary>
        public TimeSpan CommandTimeOut { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </summary>
        public TimeSpan ConnectionTimeOut { get; set; } = TimeSpan.FromSeconds(15);
        /// <summary>
        /// The service that is used to generate sql
        /// </summary>
        private ObjectToSql.Services.ObjectToSql ObjectToSql { get; }
        /// <summary>
        /// The type of database. This property is only used to control how sql is generated
        /// </summary>
        public DataBaseType DatabaseType => ObjectToSql.DatabaseType;

        //  private SqlSyntaxHelper SqlSyntaxHelper { get; }

        //    private object Lock { get; } = new object();

     
        public DatabaseAccess(DataBaseType type, string connectionString)
        {
            ConnectionString = connectionString;
            //  SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }
        public DatabaseAccess(DataBaseType type, string connectionString, TimeSpan commandTimeOut)
        {
            ConnectionString = connectionString;
            CommandTimeOut = commandTimeOut;
            //   SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }
        public DatabaseAccess(DataBaseType type, string connectionString, TimeSpan commandTimeOut, TimeSpan connectionTimeOut)
        {
            ConnectionString = connectionString;
            CommandTimeOut = commandTimeOut;
            ConnectionTimeOut = connectionTimeOut;
            //   SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }


        /// <summary>
        /// creates a new connection object
        /// </summary>
        /// <param name="openConnection"></param>
        /// <returns></returns>
        public C GetNewConnection(bool openConnection)
        {
            var connection = new C { ConnectionString = ConnectionString };
            if (openConnection)
                connection.Open();
            return connection;
        }

        /// <summary>
        /// creates a new dbcommand from the connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDbCommand GetNewCommand(IDbConnection connection, string sql, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Connection = connection;
            command.CommandType = commandType;
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            var isValidInt32 = int.TryParse(CommandTimeOut.TotalSeconds.ToString(CultureInfo.CurrentCulture), out var timeoutInSeconds);
            command.CommandTimeout = isValidInt32 ? timeoutInSeconds : int.MaxValue;
            return command;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public (IDbCommand command, IDbTransaction transaction) GetNewCommandAndTransaction(IDbConnection connection)
        {
            var command = connection.CreateCommand();
            var isValidInt32 = int.TryParse(CommandTimeOut.TotalSeconds.ToString(CultureInfo.CurrentCulture), out var timeoutInSeconds);
            command.CommandTimeout = isValidInt32 ? timeoutInSeconds : int.MaxValue;
            var transaction = connection.BeginTransaction();
            command.Transaction = transaction;
            return (command, transaction);

        }



        /// <summary>
        /// Execute an SQL Command and returns the number of rows affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// See <see cref="ExecuteNonQuery(C , string , CommandType , IEnumerable&lt;IDataParameter&gt;)"/> to perform this this action with a specified connection.
        /// <exception cref="System.InvalidOperationException"> </exception>
        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<IDataParameter> parameters = null)
        {
            using (var connection = GetNewConnection(true))
            {
                var command = GetNewCommand(connection, sql, commandType, parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Execute an SQL Command against the specified <typeparamref name="C"/>  and returns the number of rows affected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(C connection, string sql, CommandType commandType, IEnumerable<IDataParameter> parameters = null)
        {
            var command = GetNewCommand(connection, sql, commandType, parameters);
            return command.ExecuteNonQuery();

        }

        /// <summary>
        /// Executes the sql and return the 1st column of the 1st row as an object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            using (var connection = GetNewConnection(true))
            {
                return ExecuteScalar(connection, sql, commandType, parameters);
            }
        }

        /// <summary>
        /// Executes the sql and return the 1st column of the 1st row as an object
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(C connection, string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var command = GetNewCommand(connection, sql, commandType, parameters);
            return command.ExecuteScalar();

        }


        /// <summary>
        /// Executes a list of sql as in a single transaction 
        /// </summary>
        /// <param name="sqls"></param>
        /// <param name="rollbackOnException"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public int ExecuteTransaction(List<string> sqls, bool rollbackOnException, bool throwException = true)
        {
            using (var connection = GetNewConnection(true))
            {
                return ExecuteTransaction(connection, sqls, rollbackOnException, throwException);
            }
        }

        /// <summary>
        /// Executes a list of sql as in a single transaction. 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sqls"></param>
        /// <param name="rollbackOnException"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public int ExecuteTransaction(C connection, List<string> sqls, bool rollbackOnException, bool throwException = true)
        {
            var recordAffected = 0;
            if (sqls == null || !sqls.Any()) return recordAffected;

            var obj = GetNewCommandAndTransaction(connection);
            var command = obj.command;
            var transaction = obj.transaction;
            try
            {
                sqls.ForEach(delegate (string s)
                {
                    command.CommandText = s;
                    recordAffected += command.ExecuteNonQuery();
                });
                transaction.Commit();
            }
            catch (Exception)
            {
                if (rollbackOnException)
                    transaction.Rollback();
                if (throwException)
                {
                    throw;
                }
            }

            return recordAffected;
        }

        /// <summary>
        /// execute the sql and return the result as a IDataReader
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader GetDataReader(C connection, string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var command = GetNewCommand(connection, sql, commandType, parameters);
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }


        /// <summary>
        /// execute the sql and return the result as a IDataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var connection = GetNewConnection(true);
            return GetDataReader(connection, sql, commandType, parameters);
        }

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string selectSql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var reader = GetDataReader(selectSql, commandType, parameters);
            var dt = new DataTable() { };
            dt.Load(reader);
            return dt;
        }

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string selectSql)
        {
            return GetDataTable(selectSql, CommandType.Text);
        }

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable GetDataTableWithSchema(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var dt = new DataTable();
            using (var connection = GetNewConnection(true))
            {
                var command = GetNewCommand(connection, sql, commandType, parameters);
                var schema = command.ExecuteReader(CommandBehavior.SchemaOnly);
                dt.Load(schema);
                var data = command.ExecuteReader();
                dt.Load(data);
                return dt;
            }
        }

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better performance
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable GetDataTableWithKeyInfo(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var dt = new DataTable();
            using (var connection = GetNewConnection(true))
            {
                var command = GetNewCommand(connection, sql, commandType, parameters);
                var schema = command.ExecuteReader(CommandBehavior.KeyInfo);
                dt.Load(schema);
                var data = command.ExecuteReader();
                dt.Load(data);
                return dt;
            }
        }
        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTableWithSchema(string sql)
        {
            return GetDataTableWithSchema(sql, CommandType.Text, null);
        }
        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better proformance
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTableWithKeyInfo(string sql)
        {
            return GetDataTableWithKeyInfo(sql, CommandType.Text, null);
        }

        public DbParameter GetNewParameter(string parameterName, object value)
        {
            var parameter = new P { ParameterName = parameterName, Value = value };
            return parameter;
        }

        /// <summary>
        /// Attempts to open a connection to the database using the connection string provided in the constructor. 
        /// </summary>
        /// <returns></returns>
        public bool CanConnect()
        {
            try
            {
                using (var connection = GetNewConnection(true))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the specified sql and maps the results a list of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="xmlDeserializer"></param>
        /// <param name="jsonDeserializer"></param>
        /// <param name="csvDeserializer"></param>
        /// <param name="parameters"></param>
        /// <returns>The sql result mapped to a list of </returns>
        public List<T> Get<T>(string sql, CommandType commandType, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class
        {
            return GetDataReader(sql, commandType).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
        }

        /// <summary>
        /// Executes the specified sql and maps the results a list of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="xmlDeserializer"></param>
        /// <param name="jsonDeserializer"></param>
        /// <param name="csvDeserializer"></param>
        /// <param name="parameters"></param>
        /// <returns>The sql result mapped to a list of </returns>
        public List<T> Get<T>(C connection, string sql, CommandType commandType, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class
        {
            return GetDataReader(connection, sql, commandType, parameters).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
        }

        /// <summary>
        /// return a list of type of T from the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Get<T>() where T : class
        {
            return Get<T>(null, null, null);
        }



        /// <summary>
        /// return a list of type of T from the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <returns></returns>
        public List<T> Get<T>(Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            return Get<T>($"SELECT * FROM {sqlTable.FullNameWithBrackets}", CommandType.Text, xmlDeserializer, jsonDeserializer, csvDeserializer, null);
        }

        /// <summary>
        /// Executes the sql and map the results to a list of type of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> Get<T>(string sql, CommandType commandType, List<DbParameter> parameters = null) where T : class
        {
            return Get<T>(sql, commandType, null, null, null, parameters);
        }

        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from</param>
        /// <param name="actionType">type of sql to generate</param>
        /// <returns># of rows affected</returns>
        public int Execute<T>(T instance, ActionType actionType) where T : class
        {
            return Execute(instance, actionType, null, null, null, null);
        }

        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <returns></returns>
        public int Execute<T>(T instance, ActionType actionType, string tableName) where T : class
        {
            return Execute(instance, actionType, tableName, null, null, null);
        }

        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <returns></returns>
        public int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class
        {
            // var sql = (type.IsTypeAnonymousType() || type.IsTypeDynamic()) ? ObjectToSql.BuildQuery(actionType,instance, tableName ?? new SQLTable(DatabaseType, type).FullNameWithBrackets) : ObjectToSql.BuildQuery<T>( actionType, tableName ?? new SQLTable(DatabaseType, type).FullNameWithBrackets);
            var sql = ObjectToSql.BuildQuery(actionType, instance, tableName ?? new SQLTable(DatabaseType, instance.GetType()).FullNameWithBrackets);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }




        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <param name="keyFields"></param>
        /// <returns></returns>
        public int Execute<T>(T instance, ActionType actionType, string tableName, params Expression<Func<T, object>>[] keyFields) where T : class
        {
            var sql = ObjectToSql.BuildQuery<T>(actionType, tableName ?? new SQLTable(DatabaseType, instance.GetType()).FullNameWithBrackets, keyFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, null, null, null);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }


        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <param name="keyFields">Override attributes and specified which properties are keys from an expression</param>
        /// <returns></returns>
        public int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer, params Expression<Func<T, object>>[] keyFields) where T : class
        {
            var sql = ObjectToSql.BuildQuery<T>(actionType, tableName ?? new SQLTable(DatabaseType, instance.GetType()).FullNameWithBrackets,keyFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }


        /// <summary>
        ///  Creates the specified SQL from the object then executes the sql and applies the reflected values to the instance provided. This is useful when dealing with identity fields  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="outputFields">the fields to return that will reflect the values after the sql is executed  </param>
        /// <returns></returns>
        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
        {
            return ExecuteAndGetOutput(instance, actionType, null, null, null, null, null, null, outputFields);
        }

        /// <summary>
        ///  Creates the sql and applies the reflected values tothe specified SQL from the object then executes t the instance provided. This is useful when dealing with identity fields  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="xmlDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <param name="xmlSerializer">For when your storing values in the database as xml. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonSerializer">For when your storing values in the database as json. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvSerializer">For when your storing values in the database as csv. This func will be invoke to serialize any property declarated with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <param name="outputFields"></param>
        /// <returns></returns>
        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType
            , Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer
            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
            , params Expression<Func<T, object>>[] outputFields) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, instance.GetType());
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(actionType, sqlTable.FullNameWithBrackets, outputFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return GetDataReader(sql, CommandType.Text, parameters).MapTo<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
        }


        public IDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
        {
            return ExecuteAndGetOutputAsDataReader(instance, actionType, null, null, null, outputFields);
        }

        public IDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType
            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
            , params Expression<Func<T, object>>[] outputFields) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, instance.GetType());
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(actionType, sqlTable.FullNameWithBrackets, outputFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return GetDataReader(sql, CommandType.Text, parameters);
        }



    }
}
