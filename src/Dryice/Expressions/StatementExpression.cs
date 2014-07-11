using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class StatementExpression
		: Expression
	{
		public Expression Expression { get; private set; }

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.Statement;
			}
		}

		public override System.Type Type
		{
			get
			{
				return this.Expression.Type;
			}
		}

		public StatementExpression(Expression expression)
		{
			this.Expression = expression;
		}
	}
}
