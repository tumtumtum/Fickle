using System;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class ForEachExpression
		: BaseExpression
	{
		public Expression Body { get; private set; }
		public Expression Target { get; private set; }
		public ParameterExpression VariableExpression { get; private set; }
		
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
				return (ExpressionType)ServiceExpressionType.ForEach;
			}
		}

		public ForEachExpression(ParameterExpression variableExpression, Expression target, Expression body)
		{
			this.Body = body;
			this.Target = target;
			this.VariableExpression = variableExpression;
		}
	}
}
