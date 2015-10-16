using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators
{
	public class ParameterTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Tuple<Type, string>> types = new HashSet<Tuple<Type, string>>();
		private readonly Predicate<MethodDefinitionExpression> acceptMethod;
		private MethodDefinitionExpression currentMethod;

		private ParameterTypesCollector(Predicate<MethodDefinitionExpression> acceptMethod)
		{
			this.acceptMethod = acceptMethod ?? (c => true);
		}

		public static List<Tuple<Type, string>> Collect(Expression expression, Predicate<MethodDefinitionExpression> acceptMethod = null)
		{
			var collector = new ParameterTypesCollector(acceptMethod);

			collector.Visit(expression);

			return collector.types.ToList();
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			if (this.acceptMethod(method))
			{
				try
				{
					this.currentMethod = method;

					return base.VisitMethodDefinitionExpression(method);
				}
				finally
				{
					this.currentMethod = null;
				}

			}

			return method;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			types.Add(new Tuple<Type, string>(node.Type, currentMethod.Attributes["ContentFormat"]));

			return base.VisitParameter(node);
		}
	}
}
