using System;
using System.Linq;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ExpressionScope<PREV>
		: StatementBlockScope<PREV, ExpressionScope<PREV>>
		where PREV : class
	{
		public ExpressionScope(PREV previousScope, Action<Expression> complete)
			: base(previousScope, complete)
		{
		}

		internal void EndExpression() => this.complete?.Invoke(this.expressions.Single());
	}
}