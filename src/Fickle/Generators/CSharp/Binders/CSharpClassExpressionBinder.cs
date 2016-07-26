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
			var namespaceExpression = new NamespaceExpression(this.codeGenerationContext.Options.Namespace);

			var includeExpressions = new List<IncludeExpression>
			{
				FickleExpression.Include("System"),
				FickleExpression.Include("System.Collections.Generic")
			};

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup, namespaceExpression }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, expression.Body, false, expression.Attributes, expression.InterfaceTypes);
		}
	}
}

