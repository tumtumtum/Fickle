using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class GlobalExpressionScope
		: IExpressionScope<Expression>
	{
		private readonly List<Expression> expressions = new List<Expression>();

		public TypeDefinitionExpressionScope<GlobalExpressionScope> CreateTypeDefinition(Type type)
		{
			return new TypeDefinitionExpressionScope<GlobalExpressionScope>(type, this, c => this.expressions.Add(c));
		}

		public Expression End()
		{
			return new GroupedExpressionsExpression(this.expressions, GroupedExpressionsExpressionStyle.Wide);
		}
	}
}