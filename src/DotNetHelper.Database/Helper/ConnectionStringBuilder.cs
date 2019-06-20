using System;
using System.Collections.Generic;
using System.Text;
using DotNetHelper.Database.Abstractions.Enum;
using DotNetHelper.Database.Abstractions.Interface;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Abstractions.Helper
{
    public static class ConnectionStringBuilder
    {


        /// <summary>
        /// Build a SqlConnection String Based On DataSource Properties Will AutoBuild A Connection String If An Connection String Is Not Already Defined
        /// </summary>
        /// <returns>connection string</returns>
        public static string BuildConnectionString(IDataSourceDb datasource)
        {
            switch (datasource.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    return GetSqlServerConnectionString(datasource);
                case DataBaseType.MySql:
                    return GetMySqlConnectionString(datasource);

                case DataBaseType.Sqlite:
                    throw new NotImplementedException("You must reference the nuget package dotnethelper-sqlite to use this ");
                //   return GetSqliteConnectionString(datasource);;
                case DataBaseType.Oracle:
                    throw new NotImplementedException("You must reference the nuget package dotnethelper-OracleDB to use this ");

                case DataBaseType.Oledb:

                    break;
                case DataBaseType.Access95:
                    return GetAccess95ConnectionString(datasource);
                case DataBaseType.Odbc:
                    return GetOdbcConnectionString(datasource);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return datasource.ConnectionString;
        }

        /// <summary>
        /// Build a OleDbConnection String Based On DataSource Properties, If An Connection String Is Already Set Then it will that
        /// </summary>
        /// <returns>connection string</returns>
        private static string GetAccess95ConnectionString(IDataSourceDb datasource)
        {

            var PersistSecurityInfo = datasource.PersistSecurityInfo;
            var Provider = datasource.Provider;
            var DataSource = datasource.FullFileName;
            var JetOledbSystemDatabase = datasource.JetOledbSystemDatabase;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(datasource.ConnectionString)) return datasource.ConnectionString;
            var temp = PersistSecurityInfo ? "True" : "False";
            if (!string.IsNullOrEmpty(Provider))
                sb.Append(Provider.EndsWith(";") ? "Provider=" + Provider : "Provider=" + Provider + ";");
            if (!string.IsNullOrEmpty(DataSource))
                sb.Append(DataSource.EndsWith(";") ? "Data Source=" + DataSource : "Data Source=" + DataSource + ";");
            //      if (PersistSecurityInfo)
            sb.Append($"Persist Security Info={temp};");
            if (!string.IsNullOrEmpty(JetOledbSystemDatabase))
                sb.Append(JetOledbSystemDatabase.EndsWith(";") ? "Jet OLEDB:System database=" + JetOledbSystemDatabase : "Jet OLEDB:System database=" + JetOledbSystemDatabase + ";");
            if (!string.IsNullOrEmpty(UserName))
                sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
            if (!string.IsNullOrEmpty(Password))
                sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
            return sb.ToString();

        }



        private static string GetSqlServerConnectionString(IDataSourceDb datasource)
        {
            var FullFileName = datasource.FullFileName;
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var Timeout = datasource.Timeout;
            var IsSqlExpress = datasource.IsSqlExpress;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(datasource.FullFileName))
            {
                sb.Append($@"Data Source=.\SQLEXPRESS;
                          AttachDbFilename={FullFileName};
                          Integrated Security={IntegratedSecurity};
                          Connect Timeout={Timeout.TotalSeconds};
                          User Instance=True");
                return sb.ToString();
            }


            if (!IsSqlExpress)
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Server=" + Server : "Server=" + Server + ";");
                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append(" Integrated Security=SSPI;");
                if (!string.IsNullOrEmpty(UserName))
                    sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
                if (!string.IsNullOrEmpty(Password))
                    sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
                return sb.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Data Source=" + Server.Remove(Server.Length - 1, 1) : "Data Source=" + Server + "");
                sb.Append($"\\SQLExpress;");

                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Initial Catalog=" + Database : "Initial Catalog=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append("Integrated Security=True;");
                return sb.ToString();

            }

        }


        private static string GetMySqlConnectionString(IDataSourceDb datasource)
        {
            var FullFileName = datasource.FullFileName;
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var Timeout = datasource.Timeout;
            var IsSqlExpress = datasource.IsSqlExpress;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(datasource.FullFileName))
            {
                sb.Append($@"Data Source=.\SQLEXPRESS;
                          AttachDbFilename={FullFileName};
                          Integrated Security={IntegratedSecurity};
                          Connect Timeout={Timeout.TotalSeconds};
                          User Instance=True");
                return sb.ToString();
            }


            if (!IsSqlExpress)
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Server=" + Server : "Server=" + Server + ";");
                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append(" Integrated Security=SSPI;");
                if (!string.IsNullOrEmpty(UserName))
                    sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
                if (!string.IsNullOrEmpty(Password))
                    sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
                return sb.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Data Source=" + Server.Remove(Server.Length - 1, 1) : "Data Source=" + Server + "");
                sb.Append($"\\SQLExpress;");

                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "database=" + Database : "database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append("Integrated Security=True;");
                return sb.ToString();

            }

        }



        /// <summary>
        /// Build a OdbcConnection String Based On DataSource Properties, If An Connection String Is Already Set Then it will that
        /// </summary>
        /// <returns>connection string</returns>
        private static string GetOdbcConnectionString(DataSourceDb datasource)
        {
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
            //   if (!string.IsNullOrEmpty(DSN) && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password)) return $"DSN={DSN}";
            // 
            // 
            // 
            //   if (!string.IsNullOrEmpty(Driver))
            //       sb.Append(Driver.EndsWith(";") ? $"{nameof(Driver)}=" + Driver : $"{nameof(Driver)}=" + Driver + ";");
            //   if (!string.IsNullOrEmpty(Server))
            //       sb.Append(Server.EndsWith(";") ? $"servername=" + Server : $"servername=" + Server + ";");
            //   if (Port != null)
            //       sb.Append(Server.EndsWith(";") ? $"port=" + Port.Value : $"port=" + Port.Value + ";");
            //   if (!string.IsNullOrEmpty(Database))
            //       sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
            //   if (!string.IsNullOrEmpty(UserName))
            //       sb.Append(UserName.EndsWith(";") ? "UserName=" + UserName : "UserName=" + UserName + ";");
            //   if (!string.IsNullOrEmpty(Password))
            //       sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
            // 
            //   sb.Append(IntegratedSecurity ? "Integrated Security=True;" : "Integrated Security=False;");
            return sb.ToString();


        }

    }
}
