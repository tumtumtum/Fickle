using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Fickle.Expressions;
using Fickle.Model;
using Platform;

namespace Fickle.Generators.Java
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
