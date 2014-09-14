using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fickle.Generators
{
	public class ExpressionTypeCounter
		: ServiceExpressionVisitor
	{
		private int count;
		private readonly ExpressionType type;

		private ExpressionTypeCounter(ExpressionType type)
		{
			this.type = type;
		}

		public override Expression Visit(Expression expression)
		{
			if (expression.NodeType == this.type)
			{
				count++;
			}

			return base.Visit(expression);
		}

		public static int Count(Expression expression, ExpressionType type)
		{
			var counter = new ExpressionTypeCounter(type);
			
			counter.Visit(expression);

			return counter.count;
		}
	}
}
