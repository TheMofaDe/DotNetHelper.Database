using System.Data;

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

    }
}