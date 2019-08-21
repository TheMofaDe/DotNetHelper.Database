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


        /// <summary>
        /// The time in seconds to wait for the command to execute. The default is 30 seconds.
        /// </summary>
        TimeSpan CommandTimeOut { get; set; }

        /// <summary>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </summary>
        TimeSpan ConnectionTimeOut { get; set; }

        /// <summary>
        /// The type of database. This property is only used to control how sql is generated
        /// </summary>
        DataBaseType DatabaseType { get; }


        DbParameter GetNewParameter(string parameterName, object value);

        /// <summary>
        /// creates a new dbcommand from the connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDbCommand GetNewCommand(IDbConnection connection, string sql, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null);

        (IDbCommand command, IDbTransaction transaction) GetNewCommandAndTransaction(IDbConnection connection);

        /// <summary>
        /// Execute an SQL Command and returns the number of rows affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// See <see cref="DatabaseAccess{C,P}.ExecuteNonQuery(C,string,System.Data.CommandType,System.Collections.Generic.IEnumerable{System.Data.IDataParameter})"/> to perform this this action with a specified connection.
        /// <exception cref="System.InvalidOperationException"> </exception>
        int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<IDataParameter> parameters = null);



        /// <summary>
        /// Executes the sql and return the 1st column of the 1st row as an object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, CommandType commandType, List<DbParameter> parameters = null);



        /// <summary>
        /// Executes a list of sql as in a single transaction 
        /// </summary>
        /// <param name="sqls"></param>
        /// <param name="rollbackOnException"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        int ExecuteTransaction(List<string> sqls, bool rollbackOnException, bool throwException = true);


        /// <summary>
        /// execute the sql and return the result as a IDataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader GetDataReader(string sql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// execute the sql and load the results into a dataTable
        /// </summary>
        /// <param name="selectSql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
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
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable GetDataTableWithSchema(string sql, CommandType commandType, List<DbParameter> parameters = null);

        /// <summary>
        /// Applies the schema/metadata of the sql to a dataTable and populate it with the result set.
        /// If working with a large dataSet and you don't need the dataTable which columns are primary keys then use GetDataTableWithSchema for better performance
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
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


        /// <summary>
        /// Attempts to open a connection to the database using the connection string provided in the constructor. 
        /// </summary>
        /// <returns></returns>
        bool CanConnect();

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
        List<T> Get<T>(string sql, CommandType commandType, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer, List<DbParameter> parameters = null) where T : class;

        /// <summary>
        /// return a list of type of T from the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> Get<T>() where T : class;

        /// <summary>
        /// return a list of type of T from the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.XML)]</param>
        /// <param name="jsonDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.JSON)]</param>
        /// <param name="csvDeserializer">Func to invoke on properties that mark with [SqlColumnAttribute(SerializableType = SerializableType.CSV)]</param>
        /// <returns></returns>
        List<T> Get<T>(Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class;

        /// <summary>
        /// Executes the sql and map the results to a list of type of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        List<T> Get<T>(string sql, CommandType commandType, List<DbParameter> parameters = null) where T : class;

        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from</param>
        /// <param name="actionType">type of sql to generate</param>
        /// <returns># of rows affected</returns>
        int Execute<T>(T instance, ActionType actionType) where T : class;

        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <returns></returns>
        int Execute<T>(T instance, ActionType actionType, string tableName) where T : class;

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
        int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer) where T : class;


        /// <summary>
        /// Creates the specified SQL from the object then executes the sql and return the number of rows affected. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">the object to create sql from </param>
        /// <param name="actionType">type of sql to generate</param>
        /// <param name="tableName">Table name to use when generating sql </param>
        /// <param name="keyFields"></param>
        /// <returns></returns>
         int Execute<T>(T instance, ActionType actionType, string tableName, params Expression<Func<T, object>>[] keyFields) where T : class;



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
         int Execute<T>(T instance, ActionType actionType, string tableName, Func<object, string> xmlSerializer, Func<object, string> jsonSerializer, Func<object, string> csvSerializer, params Expression<Func<T, object>>[] keyFields) where T : class;




            /// <summary>
            ///  Creates the specified SQL from the object then executes the sql and applies the reflected values to the instance provided. This is useful when dealing with identity fields  
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="instance">the object to create sql from </param>
            /// <param name="actionType">type of sql to generate</param>
            /// <param name="outputFields">the fields to return that will reflect the values after the sql is executed  </param>
            /// <returns></returns>
            T ExecuteAndGetOutput<T>(T instance, ActionType actionType, params Expression<Func<T, object>>[] outputFields) where T : class;

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