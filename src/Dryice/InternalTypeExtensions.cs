using System;

namespace Fickle
{
	internal static class InternalTypeExtensions
	{
		public static bool IsNullable(this Type type)
		{
			return type.GetUnderlyingType() != null;
		}

		public static Type GetUnderlyingType(this Type type)
		{
			return DryNullable.GetUnderlyingType(type);
		}

		public static Type GetUnwrappedNullableType(this Type type)
		{
			return DryNullable.GetUnderlyingType(type) ?? type;
		}

		public static bool IsServiceType(this Type type)
		{
			return (type is DryType && ((DryType)type).ServiceClass != null);
		}

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
