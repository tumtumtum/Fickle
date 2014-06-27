using System;

namespace Sublimate
{
	public static class TypeExtensions
	{
		public static Type GetSublimateListElementType(this Type type)
		{
			var listType = type as SublimateListType;

			if (listType == null)
			{
				return null;
			}

			return listType.ListElementType;
		}
	}
}
