using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dryice
{
	internal static class EnumerableExtensions
	{
		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> items)
		{
			if (items is IList<T>)
			{
				return new ReadOnlyCollection<T>((IList<T>)items);
			}
			else
			{
				return new ReadOnlyCollection<T>(new List<T>(items));
			}
		}
	}
}
