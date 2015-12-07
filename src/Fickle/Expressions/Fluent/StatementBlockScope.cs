using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
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
		
		public CURR Call(Expression instance, Type returnType, string methodName, object args)
		{
			this.expressions.Add(FickleExpression.Call(instance, returnType, methodName, args));

			return (CURR)this;
		}

		public CURR Assign(ParameterExpression variable, Expression expression)
		{
			this.expressions.Add(Expression.Assign(variable, expression));

			return (CURR)this;
		}

		public CURR Return(Expression expression)
		{
			this.expressions.Add(FickleExpression.Return(expression));

			return (CURR)this;
		}

		public CURR Return(Action<ExpressionScope<CURR>> func)
		{
			var block = new ExpressionScope<CURR>((CURR)this, c => this.expressions.Add(FickleExpression.Return(c)));

            func(block);

			block.End();

			return (CURR)this;
		}

		public CURR Return(string variableName, Type type)
		{
			this.expressions.Add(FickleExpression.Return(Expression.Variable(type, variableName)));

			return (CURR)this;
		}

		protected virtual Expression GetExpression() => this.expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

		protected PREV End()
		{
			var result = this.GetExpression();

            this.complete?.Invoke(result);

			return this.previousScope ?? result as PREV;
		}

		public IfElseScope<CURR> If(Expression condition)
		{
			return new IfElseScope<CURR>(condition, (CURR)this, c => expressions.Add(c));
		}
	}
}