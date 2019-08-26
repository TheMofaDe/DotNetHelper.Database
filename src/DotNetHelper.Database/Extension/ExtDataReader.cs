using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using DotNetHelper.Database.Helper;
using DotNetHelper.FastMember.Extension;
using DotNetHelper.FastMember.Extension.Helpers;
using DotNetHelper.FastMember.Extension.Models;
using DotNetHelper.ObjectToSql.Attribute;
using DotNetHelper.ObjectToSql.Enum;
using DotNetHelper.ObjectToSql.Extension;
using DotNetHelper.ObjectToSql.Model;

namespace DotNetHelper.Database.Extension
{
    public static class ExtDataReader
    {



        ///// <summary>
        ///// Reads all available bytes from reader
        ///// </summary>
        ///// <param name="reader"></param>
        ///// <param name="ordinal"></param>
        ///// <returns></returns>
        //public static byte[] GetBytes(this IDataReader reader, int ordinal)
        //{
        //    byte[] result = null;

        //    if (!reader.IsDBNull(ordinal))
        //    {
        //        var size = reader.GetBytes(ordinal, 0, null, 0, 0); //get the length of data 
        //        result = new byte[size];
        //        var bufferSize = 1024;
        //        long bytesRead = 0;
        //        var curPos = 0;
        //        while (bytesRead < size)
        //        {
        //            bytesRead += reader.GetBytes(ordinal, curPos, result, curPos, bufferSize);
        //            curPos += bufferSize;
        //        }
        //    }

        //    return result;
        //}

        public static bool? HasRows(this IDataReader reader)
        {
            if (reader is DbDataReader a)
                return a.HasRows;
            return null;
        }


        private static T DataRecordToT<T>(IDataReader reader, Dictionary<string, int> readerFieldLookup, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
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
                var memberWrappers = ExtFastMember.GetMemberWrappers<T>(true);
                readerFieldLookup.ForEach(delegate (KeyValuePair<string, int> pair)
                {
                    var memberWrapper = memberWrappers.FirstOrDefault(w => w.GetNameFromCustomAttributeOrDefault() == pair.Key);
                    if (memberWrapper != null)
                    {
                        var value = reader.GetValue(pair.Value);
                        DeserializeMemberValueIfNeeded(memberWrapper, ref value, xmlDeserializer, jsonDeserializer, csvDeserializer);
                        try
                        {
                            ExtFastMember.SetMemberValue(newInstance, memberWrapper.Name, value);
                        }
                        catch (InvalidOperationException) { } // These are properties or field without a setter
                        catch (ArgumentOutOfRangeException) { }

                    }
                    else
                    {
                        // Datareader return a columns that doesn't exist in type so skip it
                    }

                });
                return newInstance;
            }

        }
        /// <summary>
        /// Maps the IDataReder to a list of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="xmlDeserializer">If the type of T have a property that is marked as xml & is stored in the database as xml. this function will execute to deserialize the value</param>
        /// <param name="jsonDeserializer">If the type of T have a property that is marked as json & is stored in the database as json. this function will execute to deserialize the value</param>
        /// <param name="csvDeserializer">If the type of T have a property that is marked as csv & is stored in the database as csv. this function will execute to deserialize the value</param>
        /// <returns></returns>
        public static List<T> MapToList<T>(this IDataReader reader, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
        {
            if (reader == null || reader.IsClosed)
            {
                return new List<T>() { };
            }
            var pocoList = new List<T>() { };

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
                    if (reader.IsDBNull(0))
                    {
                        pocoList.Add(null);
                    }
                    else
                    {
                        var value = reader.GetValue(0).ToString();
                        pocoList.Add(value as T);
                    }
                }
                reader.Close();
                reader.Dispose();
                return pocoList;
            }

            while (reader.Read())
            {
                pocoList.Add(DataRecordToT<T>(reader, readerFieldLookup, xmlDeserializer, jsonDeserializer, csvDeserializer));
            }
            reader.Close();
            reader.Dispose();
            return pocoList;
        }



        /// <summary>
        /// Maps the IDataReder to a list of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>

        /// <returns></returns>
        public static List<T> MapToList<T>(this IDataReader reader) where T : class
        {
            return reader.MapToList<T>(null, null, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="xmlDeserializer">If the type of T have a property that is marked as xml & is stored in the database as xml. this function will execute to deserialize the value</param>
        /// <param name="jsonDeserializer">If the type of T have a property that is marked as json & is stored in the database as json. this function will execute to deserialize the value</param>
        /// <param name="csvDeserializer">If the type of T have a property that is marked as csv & is stored in the database as csv. this function will execute to deserialize the value</param>
        /// <returns></returns>
        public static T MapTo<T>(this IDataReader reader, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer) where T : class
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
                    if (reader.IsDBNull(0))
                    {
                        return null;
                    }
                    else
                    {
                        var value = reader.GetValue(0).ToString();
                        return (value as T);
                    }
                }
                reader.Close();
                reader.Dispose();
            }

            while (reader.Read())
            {
                return (DataRecordToT<T>(reader, readerFieldLookup, xmlDeserializer, jsonDeserializer, csvDeserializer));
            }

            return null;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T MapTo<T>(this IDataReader reader) where T : class
        {
            return reader.MapTo<T>(null, null, null);
        }





        // TODO :: REFERENCE EXTENSION METHOD IN NEW OBJECT TO SQL VERSION
        /// <summary>
        /// Deserialize the value using the corresponding deserializer func if the <paramref name="member"/> is decorated with <c>[SqlColumn]</c> attribute and
        /// SerializableType is defined
        /// </summary>
        /// <param name="member">a wrapper of FastMember.Member </param>
        /// <param name="value">database value</param>
        /// <param name="xmlDeserializer">Func that will be used to deserialize a database value</param>
        /// <param name="jsonDeserializer">Func that will be used to deserialize a database value</param>
        /// <param name="csvDeserializer">Func that will be used to deserialize a database value</param>
        private static void DeserializeMemberValueIfNeeded(MemberWrapper member, ref object value, Func<string, Type, object> xmlDeserializer, Func<string, Type, object> jsonDeserializer, Func<string, Type, object> csvDeserializer)
        {
            var sqlAttribute = member.GetCustomAttribute<SqlColumnAttribute>();
            if (sqlAttribute != null && sqlAttribute.SerializableType != SerializableType.None)
            {
                switch (sqlAttribute.SerializableType)
                {
                    case SerializableType.Xml:
                        if (xmlDeserializer == null) throw new ArgumentNullException(nameof(xmlDeserializer), ExceptionHelper.NullDeSerializer(sqlAttribute.SerializableType, member.Name));
                        value = xmlDeserializer.Invoke(value as string, member.Type);
                        break;
                    case SerializableType.Json:
                        if (jsonDeserializer == null) throw new ArgumentNullException(nameof(jsonDeserializer), ExceptionHelper.NullDeSerializer(sqlAttribute.SerializableType, member.Name));
                        value = jsonDeserializer.Invoke(value as string, member.Type);
                        break;
                    case SerializableType.Csv:
                        if (csvDeserializer == null) throw new ArgumentNullException(nameof(csvDeserializer), ExceptionHelper.NullDeSerializer(sqlAttribute.SerializableType, member.Name));
                        value = csvDeserializer.Invoke(value as string, member.Type);
                        break;
                }
            }
        }



        public static DataTable MapToDataTable<T>(this IEnumerable<T> source) where T : class
        {
            return MapToDataTable(source, null);
        }
        public static DataTable MapToDataTable<T>(this IEnumerable<T> source, string tableName) where T : class
        {
            source.IsNullThrow(nameof(source));
            var dt = new DataTable();
            if (source.Count() == 0)
            {
                if (source is IEnumerable<IDynamicMetaObjectProvider>)
                    return dt;
            }
            List<MemberWrapper> members;
            if (source is IEnumerable<IDynamicMetaObjectProvider> listOfDynamicObjects)
            {
                members = ExtFastMember.GetMemberWrappers(listOfDynamicObjects.First()).Where(w => !w.IsMemberASerializableColumn() && !w.ShouldMemberBeIgnored()).AsList();
            }
            else
            {
                members = ExtFastMember.GetMemberWrappers<T>(true).Where(w => !w.IsMemberASerializableColumn() && !w.ShouldMemberBeIgnored()).AsList();
            }

            var keyColumns = new List<DataColumn>() { };
            members.ForEach(delegate (MemberWrapper member)
            {
                //An exception of type 'System.NotSupportedException' occurred in System.Data.dll but was not handled in user code
                // DataSet does not support System.Nullable<>.
                var dc = new DataColumn(member.GetNameFromCustomAttributeOrDefault(), member.Type.IsNullable().underlyingType); // datacolumn doesn't support nullable type so use underlying

                if (member.IsMemberAnIdentityColumn())
                    dc.AutoIncrement = true;

                if (member.IsMemberAPrimaryKeyColumn())
                    keyColumns.Add(dc);

                dt.Columns.Add(dc);
            });
            dt.PrimaryKey = keyColumns.ToArray();

            source.AsList().ForEach(delegate (T obj)
            {
                var row = dt.NewRow();
                members.ForEach(w => row[w.GetNameFromCustomAttributeOrDefault()] = w.GetValue(obj) ?? DBNull.Value);
                dt.Rows.Add(row);
            });

            dt.TableName = tableName ?? new SqlTable(DataBaseType.SqlServer, source.First().GetType()).TableName;
            return dt;
        }

    }
}
