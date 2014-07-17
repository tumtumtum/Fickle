using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dryice.Generators
{
	public class ReturnTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Type> returnTypes = new HashSet<Type>();

		private ReturnTypesCollector()
		{
		}

		public static List<Type> CollectReturnTypes(Expression expression)
		{
			var collector = new ReturnTypesCollector();

			collector.Visit(expression);

			return collector.returnTypes.ToList();
		}

		protected override Expression VisitMethodDefinitionExpression(Expressions.MethodDefinitionExpression method)
		{
			this.returnTypes.Add(method.ReturnType);

			return base.VisitMethodDefinitionExpression(method);
		}
	}
}
