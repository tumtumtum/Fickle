using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dryice.Generators
{
	public class ReturnTypesCollector
	{
		public class ReferencedTypesCollector
		   : ServiceExpressionVisitor
		{
			private readonly HashSet<Type> returnTypes = new HashSet<Type>();

			private ReferencedTypesCollector()
			{
			}

			public static List<Type> CollectReferencedTypes(Expression expression)
			{
				var collector = new ReferencedTypesCollector();

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
}
