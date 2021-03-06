﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetHelper.Database.Extension
{

	/// <summary>
	/// A collection of extensions methods put together by Joseph McNeal Jr.
	/// </summary>
	internal static class ListExtension
	{

		/// <summary>
		/// Executes a simple for each
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static ICollection<T> ForEach<T>(this ICollection<T> collection, Action<T> action)
		{
			foreach (var item in collection)
			{
				action(item);
			}
			return collection;
		}

		public static Task LoopAsync<T>(this IEnumerable<T> list, Func<T, Task> function)
		{
			return Task.WhenAll(list.Select(function));
		}

		public static async Task<List<TOut>> LoopAsyncResult<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, Task<TOut>> function)
		{
			var loopResult = await Task.WhenAll(list.Select(function));
			return loopResult.AsList();
		}

		/// <summary>
		/// Obtains the data as a list; if it is *already* a list, the original object is returned without
		/// any duplication; otherwise, ToList() is invoked.
		/// </summary>
		/// <typeparam name="T">The type of element in the list.</typeparam>
		/// <param name="source">The enumerable to return as a list.</param>
		public static List<T> AsList<T>(this IEnumerable<T> source) => (source == null || source is List<T>) ? (List<T>)source : source.ToList();




		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this ICollection source)
		{
			return source == null || source.Count <= 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this IEnumerable source)
		{
			return source == null || !source.GetEnumerator().MoveNext();
		}
	}
}