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

		private static Expression CreateDeserializeArrayMethod()
		{
			/*
			var jsonReader = Expression.Parameter(DryType.Define("JsonReader"), "reader");

			var result = DryExpression.Variable(new DryListType(currentType), "result");

			var resultNew = DryExpression.New(new DryListType(currentType), "DryListType", null);

			var whileBody = DryExpression.Block(DryExpression.Call(result, "add", DryExpression.StaticCall(currentType, "deserialize", jsonReader)));

			var whileExpression = DryExpression.While(DryExpression.Call(jsonReader, "hasNext", null), whileBody);

			var returnResult = Expression.Return(Expression.Label(), result).ToStatement();

			var methodVariables = new List<ParameterExpression>
			{
				result
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(result, resultNew).ToStatement(),
				DryExpression.Call(jsonReader, "beginArray", null),
				whileExpression,
				DryExpression.Call(jsonReader, "endArray", null),
				returnResult
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());
			
			return new MethodDefinitionExpression("deserializeArray", new List<Expression>() { jsonReader }, AccessModifiers.Public | AccessModifiers.Static, new DryListType(currentType), body, false, null, null, new List<Exception>() { new Exception() });
			 */

			return null;
		}
	}
}
