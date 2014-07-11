using System;

namespace Dryice
{
	internal static class InternalTypeExtensions
	{
		public static Type GetDryiceListElementType(this Type type)
		{
			var listType = type as DryListType;

			if (listType == null)
			{
				return null;
			}

			return listType.ListElementType;
		}
	}
}
