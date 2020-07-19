using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DotNetHelper.Database.DataSource;
using DotNetHelper.Database.Helper;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Extension
{
    public static class DbConnectionExtension
    {

	    public static ObjectToSql.Services.ObjectToSql ObjToSql<T>(this T dbConnection, bool includeNonPublicProperties = true) where T : DbConnection
	    {
		    return new ObjectToSql.Services.ObjectToSql (DatabaseTypeHelper.GetDataBaseTypeFromDBConnectionType<T>(dbConnection) ?? DataBaseType.SqlServer, includeNonPublicProperties);
	    }

		public static DB<T> DB<T>(this T dbConnection) where T : DbConnection, new()
		{
			return new DB<T>(dbConnection);
		}

        /// <summary>
		/// 
		/// </summary>
		/// <param name="connection"></param>
		/// <returns>true if the connection was force to be opened false if connection was already open</returns>
		public static bool OpenSafely(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting)
            {
                return false;
            }
            else
            {
                connection.Open();
                return true;
            }

        }

        public static void CloseSafely(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            else
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>true if the connection was force to be opened false if connection was already open</returns>
        public static async Task<bool> OpenSafelyAsync(this DbConnection connection,CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting)
            {
                return false;
            }
            else
            {
                await connection.OpenAsync(cancellationToken);
                return true;
            }

        }

       
        //public static DatabaseAccess<T> DatabaseAccess<T>(this T connection, DataBaseType? dataBaseType = null) where T : DbConnection, new()
        //{
        //    return new DatabaseAccess<T>(connection, dataBaseType);
        //}
        
    }
}