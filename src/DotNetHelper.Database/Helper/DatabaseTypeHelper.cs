using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Helper
{
    internal static class DatabaseTypeHelper
    {

        /// <summary>
        /// Gets the DataBaseType based on the DBConnection Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static DataBaseType? GetDataBaseTypeFromDBConnectionType<T>() where T : DbConnection
        {
            if (typeof(T).Name == "SqlConnection")
            {
                return DataBaseType.SqlServer;
            }
            if (typeof(T).Name == "SqliteConnection")
            {
                return DataBaseType.Sqlite;
            }
            if (typeof(T).Name == "MySqlConnection")
            {
                return DataBaseType.MySql;
            }
            if (typeof(T).Name == "OracleConnection")
            {
	            return DataBaseType.Oracle;
            }

			return null;
        }

    }
}
