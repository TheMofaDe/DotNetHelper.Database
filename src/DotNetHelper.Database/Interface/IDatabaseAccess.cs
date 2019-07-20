using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using DotNetHelper.Database.DataSource;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Helper;

namespace DotNetHelper.Database.Interface
{
    public interface IDatabaseAccess
    {
        TimeSpan CommandTimeOut { get; set; }
        TimeSpan ConnectionTimeOut { get; set; }
        DataBaseType DatabaseType { get; }
        ObjectToSql.Services.ObjectToSql ObjectToSql { get; }
        SqlSyntaxHelper SqlSyntaxHelper { get; }
       
        IDbCommand GetNewCommand(IDbConnection connection, string sql, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);
        (IDbCommand command, IDbTransaction transaction) GetNewCommandAndTransaction(IDbConnection connection);

        /// <summary>
        /// Execute an SQL Command and returns the number of rows affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// See <see cref="DatabaseAccess{C,P}.ExecuteNonQuery(C,string,System.Data.CommandType,System.Collections.Generic.IEnumerable{System.Data.IDataParameter})"/> to perform this this action with a specified connection.
        /// <exception cref="System.InvalidOperationException"> </exception>
        int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<IDataParameter> parameters = null);

       

        /// <summary>
        /// Executes the sql and return the 1st column of the 1st row as an object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, CommandType commandType, List<DbParameter> parameters = null);


        int ExecuteTransaction(List<string> sqls, bool rollbackOnException, bool throwException = true);
        
        IDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable GetDataTable(string selectSql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <returns></returns>
        DataTable GetDataTable(string selectSql);

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable GetDataTableWithSchema(string sql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better performance
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable GetDataTableWithKeyInfo(string sql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DataTable GetDataTableWithSchema(string sql);

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better proformance
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DataTable GetDataTableWithKeyInfo(string sql);

       // IDbDataParameter GetNewParameter(string parameterName, object value);
        bool CanConnect();

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
        List<T> Get<T>(string sql, CommandType commandType, Func<string,Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class;

        List<T> Get<T>() where T : class;
        List<T> Get<T>(Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class;
        List<T> Get<T>(string sql, CommandType commandType, List<DbParameter> parameters = null) where T : class;
        int Execute<T>(T instance, ActionType actionType) where T : class;
        int Execute<T>(T instance, ActionType actionType, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class;
        T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class;

        T ExecuteAndGetOutput<T>(T instance, ActionType actionType
            , Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer
            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
            , params Expression<Func<T, object>>[] outputFields) where T : class;

        IDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class;

        IDataReader ExecuteAndGetOutputAsDataReader<T>(T instance, ActionType actionType
            , Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer
            , params Expression<Func<T, object>>[] outputFields) where T : class;
    }
}