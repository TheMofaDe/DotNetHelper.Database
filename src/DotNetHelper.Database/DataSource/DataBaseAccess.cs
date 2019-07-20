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
using DotNetHelper.ObjectToSql.Helper;
using DotNetHelper.ObjectToSql.Model;

namespace DotNetHelper.Database.DataSource
{
    /// <summary>
    /// A powerful & simple class for dealing CRUD operation that doesn't required you to write sql unless you prefer to do so or need to do something advance
    /// </summary>
    /// <typeparam name="C">An implementation of IDBConnection</typeparam>
    /// <typeparam name="P">The corresponding DbParameter class to ties with the specified IDBConnection </typeparam>
    public class DatabaseAccess<C, P> : IDatabaseAccess where C : class, IDbConnection, IDisposable,  new() where P : DbParameter, new()  
    {

        private string ConnectionString { get; }
        public TimeSpan CommandTimeOut { get; set; }
        public TimeSpan ConnectionTimeOut { get; set; }
        public DataBaseType DatabaseType { get; } = DataBaseType.SqlServer;
        public ObjectToSql.Services.ObjectToSql ObjectToSql { get; private set; }
        public SqlSyntaxHelper SqlSyntaxHelper { get; private set; }

        private object Lock { get; } = new object();

        public DatabaseAccess(DataBaseType type)
        {
            SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }
        public DatabaseAccess(DataBaseType type, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("invalid connection string");
            ConnectionString = connectionString;
            SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }
        public DatabaseAccess(DataBaseType type, string connectionString, TimeSpan commandTimeOut)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("invalid connection string");
            ConnectionString = connectionString;
            CommandTimeOut = commandTimeOut;
            SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }
        public DatabaseAccess(DataBaseType type, string connectionString, TimeSpan commandTimeOut, TimeSpan connectionTimeOut)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("invalid connection string");
            ConnectionString = connectionString;
            CommandTimeOut = commandTimeOut;
            ConnectionTimeOut = connectionTimeOut;
            SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }



        public C GetNewConnection(bool openConnection)
        {
            var connection = new C { ConnectionString = ConnectionString };
            if (openConnection)
                connection.Open();
            return connection;
        }

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
        /// <param name="commandType"></param>
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
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
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
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            using (var connection = GetNewConnection(true))
            {
                var command = GetNewCommand(connection, sql, commandType, parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the sql and return the 1st column of the 1st row as an object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(C connection, string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var command = GetNewCommand(connection, sql, commandType, parameters);
            return command.ExecuteScalar();

        }

        public int ExecuteTransaction(List<string> sqls, bool rollbackOnException, bool throwException = true)
        {
            var recordAffected = 0;
            if (sqls == null || !sqls.Any()) return recordAffected;
            using (var connection = GetNewConnection(true))
            {
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
            }
            return recordAffected;
        }

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

        public IDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var connection = GetNewConnection(true);
            var command = GetNewCommand(connection, sql, commandType, parameters);
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="commandType"></param>
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
        /// <param name="commandType"></param>
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
        /// <param name="commandType"></param>
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

        public P GetNewParameter(string parameterName, object value)
        {
            var parameter = new P { ParameterName = parameterName, Value = value };
            return parameter;
        }


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
        /// <param name="commandType"></param>
        /// <param name="xmlDeserializer"></param>
        /// <param name="jsonDeserializer"></param>
        /// <param name="csvDeserializer"></param>
        /// <param name="parameters"></param>
        /// <returns>The sql result mapped to a list of </returns>
        public List<T> Get<T>(string sql, CommandType commandType, Func<string,Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class
        {
            return GetDataReader(sql, commandType).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
        }

        public List<T> Get<T>() where T : class
        {
           return  Get<T>(null,null,null);
        }
        public List<T> Get<T>(Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            return Get<T>($"SELECT * FROM {sqlTable.FullNameWithBrackets}", CommandType.Text, xmlDeserializer, jsonDeserializer, csvDeserializer, null);
        }

        public List<T> Get<T>(string sql, CommandType commandType, List<DbParameter> parameters = null) where T : class
        {
            return Get<T>(sql, commandType, null, null, null, parameters);
        }

        public int Execute<T>(T instance, ActionType actionType) where T : class
        {
            return Execute(instance, actionType, null, null, null);
        }

        public int Execute<T>(T instance, ActionType actionType, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQuery<T>(sqlTable.FullNameWithBrackets, actionType);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }


        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
        {
            return ExecuteAndGetOutput(instance, actionType, null, null, null, null,null,null,outputFields);
        }
         
        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType
            , Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer
            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
            , params Expression<Func<T, object>>[] outputFields) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(sqlTable.FullNameWithBrackets, actionType, outputFields);
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
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(sqlTable.FullNameWithBrackets, actionType, outputFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer, jsonSerializer, csvSerializer);
            return GetDataReader(sql, CommandType.Text, parameters);
        }

     
    }
}
