using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;

namespace Dryice.Generators
{
	public class ParameterTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Type> types = new HashSet<Type>();
		private readonly Predicate<MethodDefinitionExpression> acceptMethod;
		
		private ParameterTypesCollector(Predicate<MethodDefinitionExpression> acceptMethod)
		{
			this.acceptMethod = acceptMethod ?? (c => true);
		}

		public static List<Type> Collect(Expression expression, Predicate<MethodDefinitionExpression> acceptMethod = null)
		{
			var collector = new ParameterTypesCollector(acceptMethod);

			collector.Visit(expression);

			return collector.types.ToList();
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			if (this.acceptMethod(method))
			{
				return base.VisitMethodDefinitionExpression(method);
			}

			return method;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			types.Add(node.Type);

			return base.VisitParameter(node);
		}
	}
}
