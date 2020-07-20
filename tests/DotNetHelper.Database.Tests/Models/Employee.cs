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







	[Table("ObjectX")]
	public class ObjectX
	{
		[Key]
		public string ObjectXId { get; set; }
		public string Name { get; set; }
	}

	[Table("ObjectY")]
	public class ObjectY
	{
		[Key]
		public int ObjectYId { get; set; }
		public string Name { get; set; }
	}

	[Table("ObjectZ")]
	public class ObjectZ
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public interface IUser
	{
		[Key]
		int Id { get; set; }
		string Name { get; set; }
		int Age { get; set; }
	}

	public class User : IUser
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
	}

	public interface INullableDate
	{
		[Key]
		int Id { get; set; }
		DateTime? DateValue { get; set; }
	}

	public class NullableDate : INullableDate
	{
		public int Id { get; set; }
		public DateTime? DateValue { get; set; }
	}

	public class Person
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	[Table("Stuff")]
	public class Stuff
	{
		[Key]
		public short TheId { get; set; }
		public string Name { get; set; }
		public DateTime? Created { get; set; }
	}

	[Table("Automobiles")]
	public class Car
	{
		public int Id { get; set; }
		public string Name { get; set; }
		[SqlColumn(SetIsReadOnly = true)]
		public string Computed { get; set; }
	}

	[Table("Results")]
	public class Result
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Order { get; set; }
	}

	[Table("GenericType")]
	public class GenericType<T>
	{
		[Key]
		public string Id { get; set; }
		public string Name { get; set; }
	}




}