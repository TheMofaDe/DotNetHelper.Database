using System;
using System.Collections.Generic;
using System.Text;
using DotNetHelper.FastMember.Extension.Models;
using DotNetHelper.ObjectToSql.Enum;

namespace DotNetHelper.Database.Helper
{
    internal static class ExceptionHelper
    {

        public static string NullDeSerializer(SerializableType type, string propertyName)
        {

            return $"The property {propertyName} is marked with the Serializable attribute of type {type} but no implementation of a Deserializer was provided";
        }
        public static string NullSerializer(SerializableType type, string propertyName)
        {

            return $"The property {propertyName} is marked with the Serializable attribute of type {type} but no implementation of a Serializer was provided";
        }


    }
}
