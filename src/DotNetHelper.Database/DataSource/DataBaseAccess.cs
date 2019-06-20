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
    public class DatabaseAccess<C,P> where C : class, IDbConnection, IDisposable, new() where P : DbParameter, new()
    {
        private string ConnectionString { get; }
        public TimeSpan CommandTimeOut { get; set; }
        public TimeSpan ConnectionTimeOut { get; set; }
        private DataBaseType DatabaseType { get; } = DataBaseType.SqlServer;
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
        public DatabaseAccess(DataBaseType type ,string connectionString, TimeSpan commandTimeOut, TimeSpan connectionTimeOut)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new NullReferenceException("invalid connection string");
            ConnectionString = connectionString;
            CommandTimeOut = commandTimeOut;
            ConnectionTimeOut = connectionTimeOut;
            SqlSyntaxHelper = new SqlSyntaxHelper(type);
            ObjectToSql = new ObjectToSql.Services.ObjectToSql(type);
        }



        public IDbConnection GetNewConnection(bool openConnection)
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
            if(parameters != null)
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
        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<IDataParameter> parameters = null)
        {
            using (var connection = GetNewConnection(true))
            {
                var command = GetNewCommand(connection, sql, commandType, parameters);
                return command.ExecuteNonQuery();
            }
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

        public IDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var connection = GetNewConnection(true);
            var command = GetNewCommand(connection, sql, commandType, parameters);
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        public DataTable GetDataTable(string selectSql, CommandType commandType, List<DbParameter> parameters = null)
        {
            var reader = GetDataReader(selectSql, commandType, parameters);
            var dt = new DataTable() { };
            dt.Load(reader);
            return dt;
        }


        public DataTable GetDataTable(string selectSql)
        {
            return GetDataTable(selectSql, CommandType.Text);
        }

        public DataTable GetDataTableWithSchema(string sql, CommandType commandType,List<DbParameter> parameters = null)
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

        public DataTable GetDataTableWithSchema(string sql)
        {
            return GetDataTableWithSchema(sql, CommandType.Text, null);
        }

        public P GetNewParameter(string parameterName, object value)
        {
            var parameter = new P {ParameterName = parameterName, Value = value};
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


        public List<T> Get<T>(string sql, CommandType commandType, Func<object, string> xmlDeserializer, Func<object, string> jsonDeserializer, Func<object, string> csvDeserializer, List<DbParameter> parameters = null) where T : class
        {
            return GetDataReader(sql, commandType).MapToList<T>(xmlDeserializer, jsonDeserializer, csvDeserializer);
        }

        public List<T> Get<T>() where T : class
        {
            var sqlTable = new SQLTable(DatabaseType,typeof(T));
            return Get<T>($"SELECT * FROM {sqlTable.FullNameWithBrackets}", CommandType.Text, null, null, null, null);
        }
        public List<T> Get<T>(Func<object, string> xmlDeserializer, Func<object, string> jsonDeserializer, Func<object, string> csvDeserializer) where T : class
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
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQuery<T>(sqlTable.FullNameWithBrackets, actionType);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, null, null, null, true);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }

        public int Execute<T>(T instance, ActionType actionType, IColumnSerializer xmlSerializer, IColumnSerializer jsonSerializer, IColumnSerializer csvSerializer) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQuery<T>(sqlTable.FullNameWithBrackets, actionType);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer?.Serialize, jsonSerializer?.Serialize, csvSerializer?.Serialize, true);
            return ExecuteNonQuery(sql, CommandType.Text, parameters);
        }


        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(sqlTable.FullNameWithBrackets, actionType, outputFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, null, null, null, true);
            return GetDataReader(sql, CommandType.Text, parameters).MapTo<T>(null, null, null);
        }

        public T ExecuteAndGetOutput<T>(T instance, ActionType actionType, IColumnSerializer xmlSerializer, IColumnSerializer jsonSerializer, IColumnSerializer csvSerializer,  params Expression<Func<T, object>>[] outputFields) where T : class
        {
            var sqlTable = new SQLTable(DatabaseType, typeof(T));
            var sql = ObjectToSql.BuildQueryWithOutputs<T>(sqlTable.FullNameWithBrackets, actionType, outputFields);
            var parameters = ObjectToSql.BuildDbParameterList(instance, GetNewParameter, xmlSerializer?.Serialize, jsonSerializer?.Serialize, csvSerializer?.Serialize, true);
            return GetDataReader(sql, CommandType.Text, parameters).MapTo<T>(xmlSerializer?.Deserialize, jsonSerializer?.Deserialize, csvSerializer?.Deserialize);
        }




    }

}
