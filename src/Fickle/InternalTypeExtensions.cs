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
			return FickleNullable.GetUnderlyingType(type);
		}

		public static Type GetUnwrappedNullableType(this Type type)
		{
			return FickleNullable.GetUnderlyingType(type) ?? type;
		}

		public static bool IsServiceType(this Type type)
		{
			return (type is FickleType && ((FickleType)type).ServiceClass != null);
		}

		public static Type GetFickleListElementType(this Type type)
		{
			var listType = type as FickleListType;

			if (listType == null)
			{
				return null;
			}

			return listType.ListElementType;
		}
	}
}
