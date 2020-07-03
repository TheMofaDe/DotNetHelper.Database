using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DotNetHelper.Database.DataSource;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Extension
{
    public static class DBConnectionExtension
    {
        public static void OpenSafely(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting)
            {

            }
            else
            {
                connection.Open();
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
        public static async Task OpenSafelyAsync(this DbConnection connection,CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting)
            {

            }
            else
            {
                await connection.OpenAsync(cancellationToken);
            }

        }

       
        public static DatabaseAccess<T> DatabaseAccess<T>(this T connection, DataBaseType? dataBaseType = null) where T : DbConnection, new()
        {
            return new DatabaseAccess<T>(connection, dataBaseType);
        }

    }
}