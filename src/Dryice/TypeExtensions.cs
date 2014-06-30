using System;

namespace Dryice
{
	public static class TypeExtensions
	{
		public static Type GetDryiceListElementType(this Type type)
		{
			var listType = type as DryiceListType;

			if (listType == null)
			{
				return null;
			}

			return listType.ListElementType;
		}
	}
}
