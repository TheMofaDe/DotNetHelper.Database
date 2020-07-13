
using System;

namespace TestConsoleApp
{

    class Program
    {
        static void Main(string[] args)
        {
            var db = new SqlConnection("Data Source=localhost;Initial Catalog=tempdb;Integrated Security=False;User Id=sa;Password=Password12!");
			using (db)
			{
                db.
			}
            Console.WriteLine("Hello World!");
        }
    }
}
