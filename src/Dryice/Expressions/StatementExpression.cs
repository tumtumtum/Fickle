using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class StatementExpression
		: BaseExpression
	{
		public Expression Expression { get; private set; }
		public override System.Type Type { get { return typeof(void); } }
		public override ExpressionType NodeType { get { return (ExpressionType)ServiceExpressionType.Statement; } }

		public StatementExpression(Expression expression)
		{
			this.Expression = expression;
		}
	}
}
