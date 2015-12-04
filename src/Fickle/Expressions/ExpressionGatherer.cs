using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class ExpressionGatherer
		: ServiceExpressionVisitor
	{
		public ExpressionType expressionType;
		private readonly List<Expression> results = new List<Expression>();

		private ExpressionGatherer(ExpressionType expressionType)
		{
			this.expressionType = expressionType;
		}

		public static List<Expression> Gather(Expression expression, ServiceExpressionType expressionType)
		{
			return Gather(expression, (ExpressionType)expressionType);
		}

        public static List<Expression> Gather(Expression expression, ExpressionType expressionType)
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

			if (node.NodeType == this.expressionType)
			{
				this.results.Add(node);
			}

			return base.Visit(node);
		}
	}
}