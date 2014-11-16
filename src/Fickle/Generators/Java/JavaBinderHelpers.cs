using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.Java
{
	internal static class JavaBinderHelpers
	{
		public static string GetValueResponseWrapperTypeName(Type type)
		{
			var unwrappedType = type.GetUnwrappedNullableType();

			if (unwrappedType is FickleListType)
			{
				return TypeSystem.GetPrimitiveName(type.GetFickleListElementType(), true) + "ListValueResponse";
			}

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
			return (type is FickleType && ((FickleType)type).ServiceClass != null);
		}

		public static Expression CreateSerializeArrayMethod(Type arrayType)
		{
			var array = Expression.Parameter(new FickleListType(arrayType), "array");

			var jsonBuilder = FickleType.Define("DefaultJsonBuilder");

			var jsonBuilderInstance = FickleExpression.StaticCall(jsonBuilder, "instance");

			var toJsonCall = FickleExpression.Call(jsonBuilderInstance, "toJson", array);

			var defaultBody = Expression.Return(Expression.Label(), toJsonCall).ToStatement();

			var body = FickleExpression.Block(defaultBody);

			return new MethodDefinitionExpression("serializeArray", new List<Expression>() { array }, AccessModifiers.Public | AccessModifiers.Static, typeof(string), body, false, null);
		}
	}
}
