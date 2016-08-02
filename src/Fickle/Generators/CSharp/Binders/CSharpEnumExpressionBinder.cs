using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.CSharp.Binders
{
	public class CSharpEnumExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;

		private CSharpEnumExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new CSharpEnumExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var comment = new CommentExpression("This file is AUTO GENERATED");
			var header = new Expression[] {comment}.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var typeDefinitionExpression = new TypeDefinitionExpression(expression.Type, null, expression.Body, false, expression.Attributes, expression.InterfaceTypes);

			return new NamespaceExpression(this.codeGenerationContext.Options.Namespace, header, typeDefinitionExpression);
		}
	}
}
