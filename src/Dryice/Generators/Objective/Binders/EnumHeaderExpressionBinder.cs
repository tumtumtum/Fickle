using System;
using System.Linq.Expressions;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class EnumHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		private TypeDefinitionExpression currentTypeDefinition;

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new EnumHeaderExpressionBinder();

			return binder.Visit(expression);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Expression.Parameter(node.Type, currentTypeDefinition.Name.Capitalize() +  node.Name.Capitalize());
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			try
			{
				currentTypeDefinition = expression;

				var body = this.Visit(expression.Body);
				
				return new TypeDefinitionExpression(expression.Type, null, body, false, null, null);
			}
			finally
			{
				currentTypeDefinition = null;
			}
		}
	}
}
