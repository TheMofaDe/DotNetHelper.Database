using System;
using System.Collections.Generic;
using System.Text;
using DotNetHelper.Database.Tests.Models;

namespace DotNetHelper.Database.Tests.MockData
{
    public static class MockEmployee
    {
        public static HashSet<Employee> Hashset = new HashSet<Employee>(new List<Employee>()
        {
            new Employee()
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Blue"
                ,IdentityField = 1
            },
            new Employee()
            {
                FirstName = "John 2",
                LastName = "Doe 2",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Green"
                ,IdentityField = 2
            },
            new Employee()
            {
                FirstName = "John 3",
                LastName = "Doe 3",
                DateOfBirth = new DateTime(1994, 07, 22),
                FavoriteColor = "Yellow"
                ,IdentityField = 3
            }
        });



    }
}
