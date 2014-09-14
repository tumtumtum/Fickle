using System;
using Platform;

namespace Fickle.Generators.Objective
{
	internal static class ObjectiveBinderHelpers
	{
		public static bool IsObjcValueType(Type type)
		{
			return type.IsNumericType() || type == typeof(bool) || type.IsEnum;
		}

		public static bool NeedsValueResponseWrapper(Type type)
		{
			return TypeSystem.IsPrimitiveType(type) || type is DryListType;
		}

		public static bool ValueResponseValueNeedsBoxing(Type type)
		{
			return type.IsNumericType() || type == typeof(bool);
		}

		public static string GetValueResponseWrapperTypeName(Type type)
		{
			var unwrappedType = type.GetUnwrappedNullableType();

			if (unwrappedType.IsNumericType(true) || unwrappedType == typeof(bool) || (unwrappedType.IsEnum && type.IsNullable()))
			{
				return "NumberValueResponse";
			}
			else if (unwrappedType is DryListType)
			{
				return "ArrayValueResponse";
			}
			else 
			{
				return TypeSystem.GetPrimitiveName(unwrappedType, true) + "ValueResponse";
			}
		}

		public static Type GetWrappedResponseType(CodeGenerationContext context, Type type)
		{
			if (TypeSystem.IsPrimitiveType(type) ||  type is DryListType)
			{
				return context.ServiceModel.GetServiceType(GetValueResponseWrapperTypeName(type));
			}
			
			return type;
		}

		public static bool TypeIsServiceClass(Type type)
		{
			return (type is DryType && ((DryType)type).ServiceClass != null);
		}
	}
}
