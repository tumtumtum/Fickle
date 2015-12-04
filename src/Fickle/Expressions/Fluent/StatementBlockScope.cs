using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ExpressionBlock<PREV>
		: StatementBlockScope<PREV, ExpressionBlock<PREV>>
		where PREV : class
	{
		public ExpressionBlock(PREV previousScope, Action<Expression> complete)
			: base(previousScope, complete)
		{
		}

		internal void EndExpression()
		{
			this.complete?.Invoke(this.expressions.Single());
		}
	}

	public class StatementBlockScope<PREV, CURR>
		where PREV : class
		where CURR : StatementBlockScope<PREV, CURR>
	{
		protected readonly PREV previousScope;
		protected readonly Action<Expression> complete;
		protected List<Expression> expressions = new List<Expression>();

		protected StatementBlockScope(PREV previousScope, Action<Expression> complete)
		{
			this.previousScope = previousScope;
			this.complete = complete;
		}

		protected PREV EndWithResult(Expression result)
		{
			if (this.previousScope == default(PREV) && this.complete == null)
			{
				return (PREV)(object)result;
			}

			this.complete?.Invoke(result);

			return this.previousScope;
		}

		public CURR Call(Expression instance, Type returnType, string methodName, object args)
		{
			this.expressions.Add(FickleExpression.Call(instance, returnType, methodName, args));

			return (CURR)this;
		}

		public CURR Return(Expression expression)
		{
			this.expressions.Add(FickleExpression.Return(expression));

			return (CURR)this;
		}

		public CURR Return(Action<ExpressionBlock<CURR>> func)
		{
			var block = new ExpressionBlock<CURR>((CURR)this, c => this.expressions.Add(FickleExpression.Return(c)));

            func(block);

			block.End();

			return (CURR)this;
		}

		public CURR Return(string variableName, Type type)
		{
			this.expressions.Add(FickleExpression.Return(Expression.Variable(type, variableName)));

			return (CURR)this;
		}

		protected PREV End()
		{	
			this.complete?.Invoke(this.expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return this.previousScope;
		}

		public IfElseScope<StatementBlockScope<PREV, CURR>> If(Expression condition)
		{
			return new IfElseScope<StatementBlockScope<PREV, CURR>>(condition, this, c => expressions.Add(c));
		}
	}
}