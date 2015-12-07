using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ElseScope<T>
		: StatementBlockScope<T, ElseScope<T>>
		where T : class
	{
		public ElseScope(T ifElseScope, Action<Expression> complete)
			: base(ifElseScope, complete)
		{
		}

		public virtual T EndIf() => base.End();
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
		
		public IfElseScope<T> ElseIf(Expression condition)
		{
			this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(this.currentCondition, this.GetExpression()));

			this.currentCondition = condition;
			this.expressions = new List<Expression>();

			return this;
		}

		public ElseScope<T> Else()
		{
			this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(this.currentCondition, this.GetExpression()));

			this.currentCondition = null;
			this.expressions = new List<Expression>();

			return new ElseScope<T>(this.previousScope, c =>
			{
				this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(null, c));

				this.EndIf();
			});
		}

		public virtual T EndIf()
		{
			Expression current = null;

			if (this.currentCondition != null)
			{
				this.conditionsAndExpressions.Add(new Tuple<Expression, Expression>(this.currentCondition, this.expressions.ToGroupedExpression()));
			}

			for (var i = this.conditionsAndExpressions.Count - 1; i >= 0; i--)
			{
				var item = this.conditionsAndExpressions[i];

				if (i == this.conditionsAndExpressions.Count - 1)
				{
					current = item.Item1 == null ? (Expression)item.Item2.ToBlock() : Expression.IfThen(item.Item1, item.Item2.ToBlock());

					continue;
				}

				if (current == null)
				{
					throw new InvalidOperationException();
				}

				current = Expression.IfThenElse(item.Item1, item.Item2.ToBlock(), current);
			}

			current = current ?? Expression.Empty();

			this.complete(current);

			return this.previousScope;
		}
	}
}