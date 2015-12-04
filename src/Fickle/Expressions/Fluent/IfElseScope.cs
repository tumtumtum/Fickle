using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ElseScope<T>
		: StatementBlockScope<IfElseScope<T>, ElseScope<T>>
		where T : class
	{
		public ElseScope(IfElseScope<T> ifElseScope, Action<Expression> complete)
			: base(ifElseScope, complete)
		{
		}

		public virtual IfElseScope<T> EndIf() => base.End();
	}

	public class IfElseScope<T>
		: StatementBlockScope<T, IfElseScope<T>>
		where T : class
	{
		private Expression currentCondition;
		private readonly List<Tuple<Expression, Expression>> conditionsAndExpressions = new List<Tuple<Expression, Expression>>();
		
		public IfElseScope(Expression condition, T previousScope, Action<Expression> complete)
			: base(previousScope, complete)
		{
			this.currentCondition = condition;
		}

		public IfElseScope<T> Then(Expression expression)
		{
			this.currentCondition = expression;
			this.expressions = new List<Expression>();

			return this;
		}
		
		public IfElseScope<T> ElseIf(Expression condition)
		{
			this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(this.currentCondition, new GroupedExpressionsExpression(this.expressions, GroupedExpressionsExpressionStyle.Wide)));

			this.currentCondition = condition;
			this.expressions = new List<Expression>();

			return this;
		}

		public ElseScope<T> Else()
		{
			this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(this.currentCondition, new GroupedExpressionsExpression(this.expressions, GroupedExpressionsExpressionStyle.Wide)));

			this.currentCondition = null;
			this.expressions = new List<Expression>();

			return new ElseScope<T>(this, c =>
			{
				this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(null, c));

				this.EndIf();
			});
		}

		public virtual T EndIf()
		{
			Expression current = null;

			for (var i = this.conditionsAndExpressions.Count - 1; i >= 0; i--)
			{
				var item = this.conditionsAndExpressions[i];

				if (i == this.conditionsAndExpressions.Count - 1)
				{
					if (item.Item1 == null)
					{
						current = item.Item2;
					}
					else
					{
						current = Expression.IfThen(item.Item1, item.Item2);
					}

					continue;
				}

				if (current == null)
				{
					throw new InvalidOperationException();
				}

				current = Expression.IfThenElse(item.Item1, item.Item2, current);
			}

			this.complete(current);

			return this.previousScope;
		}
	}
}