using System.Collections.Generic;

namespace Fickle.WebApi
{
	internal static class InternalEnumerableExtensions
	{
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
		{
			if (items is HashSet<T>)
			{
				return (HashSet<T>)items;
			}
			else
			{
				return new HashSet<T>(items);
			}
		}
	}
}
