using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Sublimate.Expressions
{
	public class StatementsExpression
		: GroupedExpressionsExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.Statement;
			}
		}

		public StatementsExpression(Expression expression)
			: this(new ReadOnlyCollection<Expression>(new List<Expression>() {expression }))
		{	
		}

		public StatementsExpression(IEnumerable<Expression> expressions)
			: this(new ReadOnlyCollection<Expression>(expressions.ToList()))
		{	
		}

		public StatementsExpression(ReadOnlyCollection<Expression> expressions)
			: base(expressions)
		{
		}
	}
}
