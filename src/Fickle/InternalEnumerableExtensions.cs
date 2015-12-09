using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fickle
{
	internal static class InternalEnumerableExtensions
	{
		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> items)
		{
			var collection = items as ReadOnlyCollection<T>;

			if (collection != null)
			{
				return collection;
			}

			var list = items as IList<T>;

			if (list != null)
			{
				return new ReadOnlyCollection<T>(list);
			}

			return new ReadOnlyCollection<T>(items.ToList());
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
		{
			var set = items as HashSet<T>;

			return set ?? new HashSet<T>(items);
		}
	}
}
