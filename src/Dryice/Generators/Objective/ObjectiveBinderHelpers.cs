using System;
using Platform;

namespace Dryice.Generators.Objective
{
	internal static class ObjectiveBinderHelpers
	{
		public static string GetValueResponseWrapperTypeName(Type type)
		{
			var unwrappedType = type.GetUnwrappedNullableType();

			if (unwrappedType.IsNumericType(true) || unwrappedType == typeof(bool) || (unwrappedType.IsEnum && type.IsNullable()))
			{
				return "NumberValueResponse";
			}
			else 
			{
				return TypeSystem.GetPrimitiveName(unwrappedType, true) + "ValueResponse";
			}
		}

		public static Type GetWrappedResponseType(CodeGenerationContext context, Type type)
		{
			if (TypeSystem.IsPrimitiveType(type))
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
