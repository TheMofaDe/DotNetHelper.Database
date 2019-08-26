using System;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetHelper.Database.Extension
{




    public static class TypeExtension
    {


        /// <summary>
        /// returns a tuple with the first value indicating if the specified type is a Nullable type and the second value indicating the underlyingtype
        /// if type is not nullable the underlyingType with be the specified type provided
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static (bool isNullableT, Type underlyingType) IsNullable(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var childType = Nullable.GetUnderlyingType(type);
                return (true, childType);
            }
            return (false, type);
        }

        /// <summary>
        /// return true if the type is assignable form IDynamicMetaObjectProvider
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeDynamic(this Type type)
        {
            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);
        }

        /// <summary>
        /// return true if type is anonymous
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeAnonymousType(this Type type)
        {
            // https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
            return System.Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

    }
}