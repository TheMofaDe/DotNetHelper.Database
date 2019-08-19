using System;
using System.Collections.Generic;
using System.Text;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Helper
{
    public static class DBProviderHelper
    {
        public static Dictionary<DataBaseType, string> Map { get; } = new Dictionary<DataBaseType, string>()
        {
             {DataBaseType.SqlServer, "System.Data.SqlClient" }
            ,{DataBaseType.MySql, "MySql.Data.MySqlClient" }
            ,{DataBaseType.Sqlite, " System.Data.SQLite" }
            ,{DataBaseType.Oracle, "Oracle.ManagedDataAccess.Client" }
        };

    }
}
