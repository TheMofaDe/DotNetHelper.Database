using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using DotNetHelper.FastMember.Extension;
using DotNetHelper.FastMember.Extension.Helpers;
using DotNetHelper.FastMember.Extension.Models;
using DotNetHelper.ObjectToSql.Attribute;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Extension;

namespace DotNetHelper.Database.Extension
{
    public static class ExtDataReader
    {



        /// <summary>
        /// Reads all available bytes from reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this IDataReader reader, int ordinal)
        {
            byte[] result = null;

            if (!reader.IsDBNull(ordinal))
            {
                var size = reader.GetBytes(ordinal, 0, null, 0, 0); //get the length of data 
                result = new byte[size];
                var bufferSize = 1024;
                long bytesRead = 0;
                var curPos = 0;
                while (bytesRead < size)
                {
                    bytesRead += reader.GetBytes(ordinal, curPos, result, curPos, bufferSize);
                    curPos += bufferSize;
                }
            }

            return result;
        }

        public static bool? HasRows(this IDataReader reader)
        {
            if (reader is DbDataReader a)
                return a.HasRows;
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> MapToList<T>(this IDataReader reader, Func<object, string> xmlDeserializer, Func<object, string> jsonDeserializer, Func<object, string> csvDeserializer) where T : class
        {
            if (reader == null || reader.IsClosed)
            {
                return new List<T>() { };
            }
            var pocoList = new List<T>(){};

            // Cache the field names in the reader for use in our while loop for efficiency.
            var readerFieldLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // store name and ordinal
            for (var i = 0; i < reader.FieldCount; i++)
            {
                readerFieldLookup.Add(reader.GetName(i), i);
            }
            if (typeof(T) == typeof(string))
            {
                while (reader.Read())
                {
                    pocoList.Add(reader.GetString(0) as T);
                }
                reader.Close();
                reader.Dispose();
                return pocoList;
            }

            while (reader.Read())
            {
                if (typeof(T).IsTypeDynamic())
                {
                    var dynamicInstance = new ExpandoObject();
                    readerFieldLookup.ForEach(delegate (KeyValuePair<string, int> pair)
                    {
                        var value = reader.GetValue(pair.Value);
                        ExtFastMember.SetMemberValue(dynamicInstance, pair.Key, value);
                    });
                    pocoList.Add(dynamicInstance as T);
                }
                else
                {
                    var newInstance = New<T>.Instance();
                    readerFieldLookup.ForEach(delegate (KeyValuePair<string, int> pair)
                    {
                        var value = reader.GetValue(pair.Value);
                        var memberWrappers = ExtFastMember.GetMemberWrappers<T>(true);
                        memberWrappers
                            .Where(p => readerFieldLookup.ContainsKey(p.GetNameFromCustomAttributeOrDefault())).ToList()
                            .ForEach(delegate (MemberWrapper p)
                            {
                                DeserializeMemberValueIfNeeded(p, ref value, xmlDeserializer, jsonDeserializer, csvDeserializer);
                                try { 
                                ExtFastMember.SetMemberValue(newInstance, pair.Key, value);
                                }
                                catch (InvalidOperationException) { } // These are properties or field without a setter
                                catch (ArgumentOutOfRangeException) { }
                            });
                    });
                    pocoList.Add(newInstance);
                }
            }
            reader.Close();
            reader.Dispose();
            return pocoList;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T MapTo<T>(this IDataReader reader, Func<object, string> xmlDeserializer, Func<object, string> jsonDeserializer, Func<object, string> csvDeserializer) where T : class
        {
            if (reader == null || reader.IsClosed)
            {
                return null;
            }
            // Cache the field names in the reader for use in our while loop for efficiency.
            var readerFieldLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // store name and ordinal
            for (var i = 0; i < reader.FieldCount; i++)
            {
                readerFieldLookup.Add(reader.GetName(i), i);
            }
            if (typeof(T) == typeof(string))
            {
                while (reader.Read())
                {
                    return reader.GetString(0) as T;
                }
                reader.Close();
                reader.Dispose();
            }

            while (reader.Read())
            {
                if (typeof(T).IsTypeDynamic())
                {
                    var dynamicInstance = new ExpandoObject();
                    readerFieldLookup.ForEach(delegate (KeyValuePair<string, int> pair)
                    {
                        var value = reader.GetValue(pair.Value);
                        ExtFastMember.SetMemberValue(dynamicInstance, pair.Key, value);
                    });
                    return dynamicInstance as T;
                }
                else
                {
                    var newInstance = New<T>.Instance();
                    readerFieldLookup.ForEach(delegate(KeyValuePair<string, int> pair)
                    {
                        var value = reader.GetValue(pair.Value);
                        var memberWrappers = ExtFastMember.GetMemberWrappers<T>(true);
                        memberWrappers
                            .Where(p => readerFieldLookup.ContainsKey(p.GetNameFromCustomAttributeOrDefault())).ToList()
                            .ForEach(delegate(MemberWrapper p)
                            {
                                DeserializeMemberValueIfNeeded(p, ref value, xmlDeserializer, jsonDeserializer, csvDeserializer);
                                try { 
                                ExtFastMember.SetMemberValue(newInstance, pair.Key, value);
                                }
                                catch (InvalidOperationException) { } // These are properties or field without a setter
                                catch (ArgumentOutOfRangeException) { }

                            });
                    });
                    return newInstance;
                }

            }
            return default(T);
        }



        // TODO :: REFERENCE EXTENSION METHOD IN NEW OBJECT TO SQL VERSION
        private static void DeserializeMemberValueIfNeeded(MemberWrapper member,ref object value, Func<object, string> xmlDeserializer, Func<object, string> jsonDeserializer, Func<object, string> csvDeserializer)
        {
            var sqlAttribute = member.GetCustomAttribute<SqlColumnAttribute>();
            if (sqlAttribute != null && sqlAttribute.SerializableType != SerializableType.NONE)
            {
                switch (sqlAttribute.SerializableType)
                {
                    case SerializableType.XML:
                        value = xmlDeserializer.Invoke(value);
                        break;
                    case SerializableType.JSON:
                        value = jsonDeserializer.Invoke(value);
                        break;
                    case SerializableType.CSV:
                        value = csvDeserializer.Invoke(value);
                        break;
                }
            }
        }




    }
}
