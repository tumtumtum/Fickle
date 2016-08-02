using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.CSharp.Binders
{
	public class CSharpClassExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;

		private CSharpClassExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new CSharpClassExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var comment = new CommentExpression("This file is AUTO GENERATED");

			var includeExpressions = new List<IncludeExpression>
			{
				FickleExpression.Include("System"),
				FickleExpression.Include("System.Collections.Generic")
			};

			foreach (var include in this.codeGenerationContext.Options.Includes)
			{
				includeExpressions.Add(FickleExpression.Include(include));
			}

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var typeDefinitionExpression = new TypeDefinitionExpression(expression.Type, null, expression.Body, false, expression.Attributes, expression.InterfaceTypes);

			return new NamespaceExpression(this.codeGenerationContext.Options.Namespace, header, typeDefinitionExpression);
		}
	}
}

