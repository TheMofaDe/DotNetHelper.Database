using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetHelper.ObjectToSql.Attribute;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Tests.Models
{

    public class SpecialDataTypeTable
    {
        //  public TimeSpan TimeSpan { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public Guid Id { get; set; }
        public byte[] Bytes { get; set; }
    }

    public class Employee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdentityField { get; set; }

        [MaxLength(400)]
        public string FirstName { get; set; }

        [MaxLength(400)]
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => FirstName + " " + LastName;
        [SqlColumn(MapTo = "DOB")]
        public DateTime DateOfBirth { get; set; }
        [MaxLength(400)]
        public string FavoriteColor { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Employee()
        {

        }



    }


    [Table("Employee2")]
    public class EmployeeSerialize
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int IdentityField { get; set; }

        [SqlColumn(SerializableType = SerializableType.Json)]
        public Employee EmployeeAsJson { get; set; }

        [SqlColumn(SerializableType = SerializableType.Json)]
        public List<Employee> EmployeeListAsJson { get; set; }


        [SqlColumn(SerializableType = SerializableType.Csv)]
        public Employee EmployeeAsCsv { get; set; }

        [SqlColumn(SerializableType = SerializableType.Csv)]
        public List<Employee> EmployeeListAsCsv { get; set; }


        [SqlColumn(SerializableType = SerializableType.Xml)]
        public Employee EmployeeAsXml { get; set; }

        [SqlColumn(SerializableType = SerializableType.Xml)]
        public List<Employee> EmployeeListAsXml { get; set; }

        public EmployeeSerialize()
        {

        }



    }




}
