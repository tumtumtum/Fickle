using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class ExpressionGatherer
		: ServiceExpressionVisitor
	{
		public ServiceExpressionType expressionType;
		private readonly List<Expression> results = new List<Expression>();

		private ExpressionGatherer(ServiceExpressionType expressionType)
		{
			this.expressionType = expressionType;
		}

		public static List<Expression> Gather(Expression expression, ServiceExpressionType expressionType)
		{
			var gatherer = new ExpressionGatherer(expressionType);

			gatherer.Visit(expression);

			return gatherer.results;
		}

		public override Expression Visit(Expression node)
		{
			if (node == null)
			{
				return null;
			}

			if (node.NodeType == (ExpressionType)this.expressionType)
			{
				this.results.Add(node);
			}

			return base.Visit(node);
		}
	}
}