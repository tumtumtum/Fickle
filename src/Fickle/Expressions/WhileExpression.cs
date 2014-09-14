using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class WhileExpression
		: BaseExpression
	{
		public Expression Body { get; private set; }
		public Expression Test { get; private set; }
		
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

		public WhileExpression(Expression test, Expression body)
		{
			this.Body = body;
			this.Test = test;
		}
	}
}
