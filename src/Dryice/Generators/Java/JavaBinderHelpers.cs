using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java
{
	internal static class JavaBinderHelpers
	{
		public static string GetValueResponseWrapperTypeName(Type type)
		{
			return TypeSystem.GetPrimitiveName(type, true) + "ValueResponse";
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

		public static Expression CreateSerializeArrayMethod(Type arrayType)
		{
			var array = Expression.Parameter(new DryListType(arrayType), "array");

			var jsonBuilder = DryType.Define("DefaultJsonBuilder");

			var jsonBuilderInstance = DryExpression.StaticCall(jsonBuilder, "instance");

			var toJsonCall = DryExpression.Call(jsonBuilderInstance, "toJson", array);

			var defaultBody = Expression.Return(Expression.Label(), toJsonCall).ToStatement();

			var body = DryExpression.Block(defaultBody);

			return new MethodDefinitionExpression("serializeArray", new List<Expression>() { array }, AccessModifiers.Public | AccessModifiers.Static, typeof(string), body, false, null);
		}
	}
}
