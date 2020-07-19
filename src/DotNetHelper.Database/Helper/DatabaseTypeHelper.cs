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
		internal static DataBaseType? GetDataBaseTypeFromDBConnectionType<T>(DbConnection dbConnection = null) where T : DbConnection
		{
			var type = dbConnection?.GetType() ?? typeof(T);

			if (type.Name == "SqlConnection")
			{
				return DataBaseType.SqlServer;
			}
			if (type.Name == "SqliteConnection")
			{
				return DataBaseType.Sqlite;
			}
			if (type.Name == "MySqlConnection")
			{
				return DataBaseType.MySql;
			}
			if (type.Name == "OracleConnection")
			{
				return DataBaseType.Oracle;
			}
			return null;
		}

	}
}