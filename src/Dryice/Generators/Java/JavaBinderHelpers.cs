using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java
{
	internal static class JavaBinderHelpers
	{
		public static string GetValueResponseWrapperTypeName(Type type)
		{
			if (type.IsNumericType(true) || type.GetUnwrappedNullableType() == typeof(bool))
			{
				return "NumberValueResponse";
			}
			else
			{
				return TypeSystem.GetPrimitiveName(type) + "ValueResponse";
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
