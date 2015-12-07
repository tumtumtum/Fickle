using System;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class BlockScope<PREV>
		: StatementBlockScope<PREV, BlockScope<PREV>>
		where PREV:class
	{
		private readonly ParameterExpression[] variables;

		public BlockScope(ParameterExpression[] variables, PREV previousScope = null, Action<Expression> complete = null)
			: base(previousScope, complete)
		{
			this.variables = variables;
		}

		protected override Expression GetExpression() => 
			Expression.Block(this.variables, this.expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

		public PREV EndBlock() => base.End();
	}
}