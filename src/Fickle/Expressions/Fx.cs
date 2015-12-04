using System;
using Fickle.Expressions.Fluent;

namespace Fickle.Expressions
{
	public static class Fx
	{
		public static TypeDefinitionExpressionScope<TypeDefinitionExpression> DefineType(Type type)
		{
			return new TypeDefinitionExpressionScope<TypeDefinitionExpression>(type);
		}

		public static TypeDefinitionExpressionScope<TypeDefinitionExpression> DefineType(string name)
		{
			return new TypeDefinitionExpressionScope<TypeDefinitionExpression>(FickleType.Define(name));
		}

		public static MethodDefinitionScope<MethodDefinitionExpression> DefineMethod(string name, string returnType, object parameters)
		{
			return new MethodDefinitionScope<MethodDefinitionExpression>(name, FickleType.Define(returnType), parameters);
		}

		public static MethodDefinitionScope<MethodDefinitionExpression> DefineMethod(string name, Type returnType, object parameters)
		{
			return new MethodDefinitionScope<MethodDefinitionExpression>(name, returnType, parameters);
		}

		public static T DefineExpression<T>()
		{
			return default(T);
		}
	}
}
