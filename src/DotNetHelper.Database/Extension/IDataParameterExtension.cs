using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotNetHelper.Database.Extension
{
	public static class IDataParameterExtension
	{
		/// <summary>
		/// Adds the elements of the specified collection to the end of the collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="collectionToAdd">elements to add to collection</param>
		public static void AddRange<T>(this IDataParameterCollection collection, IEnumerable<T> collectionToAdd)
		{
			var toAdd = collectionToAdd as IList<T> ?? collectionToAdd.ToList();
			for (var i = 0; i < toAdd.ToList().Count(); i++)
			{
				collection.Add(toAdd[i]);
			}
		}

	}
}