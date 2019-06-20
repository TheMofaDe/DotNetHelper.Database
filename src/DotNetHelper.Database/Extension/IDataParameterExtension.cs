using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotNetHelper.Database.Extension
{
    public static class IDataParameterExtension
    {
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
