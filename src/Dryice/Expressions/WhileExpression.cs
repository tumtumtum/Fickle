using System;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class WhileExpression
		: BaseExpression
	{
		public Expression Body { get; private set; }
		public Expression Condition { get; private set; }
		
		public override Type Type
		{
			get
			{
				return typeof(void);
			}
		}

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.While;
			}
		}

		public WhileExpression(Expression condition, Expression body)
		{
			this.Body = body;
			this.Condition = condition;
		}
	}
}
