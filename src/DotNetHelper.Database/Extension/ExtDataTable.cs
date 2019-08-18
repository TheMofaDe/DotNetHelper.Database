using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using DotNetHelper.FastMember.Extension;
using DotNetHelper.FastMember.Extension.Helpers;
using DotNetHelper.FastMember.Extension.Models;
using DotNetHelper.ObjectToSql.Extension;

namespace DotNetHelper.Database.Extension
{
    public static class ExtDataTable
    {
        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order </remarks>
        public static void SetColumnsOrder(this DataTable table, params string[] columnNames)
        {
            var listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (var colName in columnNames)
            {
                if (!table.Columns.Contains(colName))
                {
                    listColNames.Remove(colName);
                }
            }

            foreach (var colName in listColNames)
            {
                table.Columns[colName].SetOrdinal(listColNames.IndexOf(colName));
            }

        }

        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static bool SetColumnPosition(this DataTable table, string columnName, int position)
        {
            if (!table.Columns.Contains(columnName))
            {
                return false;
            }
            table.Columns[columnName].SetOrdinal(position);
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="useAttributeName">if true when mapping datarow columns to T instance. Attribute mapto name will be used instead of property name if exist</param>
        /// <returns></returns>
        public static T MapTo<T>(this DataRow row, bool useAttributeName = true) where T : class
        {
            var obj = New<T>.Instance();
            if (typeof(T).IsTypeDynamic())
            {
                dynamic expandoObject = new ExpandoObject();
                foreach (DataColumn column in row.Table.Columns)
                {
                    var value = row[column];
                    ExtFastMember.SetMemberValue(expandoObject, column.ColumnName, value == DBNull.Value ? null : value);

                }
                return expandoObject;
            }
            ExtFastMember.GetMemberWrappers<T>(true).ForEach(delegate (MemberWrapper wrapper)
            {
                var columnName = useAttributeName ? wrapper.GetNameFromCustomAttributeOrDefault() : wrapper.Name;
                if (row.Table.Columns.Contains(columnName))
                {
                    var value = row[columnName];
                    if (value == DBNull.Value) value = null;
                    try
                    {
                        ExtFastMember.SetMemberValue(obj, wrapper.Name, value);
                    }
                    catch (InvalidOperationException) { } // These are properties or field without a setter
                    catch (ArgumentOutOfRangeException) { }
                }
            });
            return obj;
        }


        public static List<T> MapToList<T>(this DataTable dataTable, bool useAttributeName = true) where T : class
        {
            dataTable.IsNullThrow(nameof(dataTable));
            var list = new List<T>() { };
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(row.MapTo<T>(useAttributeName));
            }

            return list;
        }

    }
}
