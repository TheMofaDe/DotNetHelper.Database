using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetHelper.Database.Tests
{
    public class BaseTest
    {
        public string WorkingDirectory { get; }

        public BaseTest()
        {
            WorkingDirectory = $"{Environment.CurrentDirectory}";
        }

    }
}
