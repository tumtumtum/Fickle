//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System.Linq.Expressions;
using Sublimate.Expressions;
using ExpressionVisitor = Platform.Linq.ExpressionVisitor;

namespace Sublimate
{
	public class ServiceExpressionVisitor
		: ExpressionVisitor
	{
		protected override Expression Visit(Expression expression)
		{
			switch ((int)expression.NodeType)
			{
				case (int)ServiceExpressionType.CodeBlock:
					return this.VisitBlockExpression((CodeBlockExpression)expression);
				case (int)ServiceExpressionType.GroupedExpressions:
					return this.VisitGroupedExpressionsExpression((GroupedExpressionsExpression)expression);
				case (int)ServiceExpressionType.MethodDefinition:
					return this.VisitMethodDefinitionExpression((MethodDefinitionExpression)expression);
				case (int)ServiceExpressionType.ParameterDefinition:
					return this.VisitParameterDefinitionExpression((ParameterDefinitionExpression)expression);
				case (int)ServiceExpressionType.PropertyDefinition:
					return this.VisitPropertyDefinitionExpression((PropertyDefinitionExpression)expression);
				case (int)ServiceExpressionType.TypeDefinition:
					return this.VisitTypeDefinitionExpression((TypeDefinitionExpression)expression);
				case (int)ServiceExpressionType.IncludeStatement:
					return this.VisitIncludeStatementExpresson((IncludeStatementExpression)expression);
				case (int)ServiceExpressionType.ReferencedType:
					return this.VisitReferencedTypeExpresson((ReferencedTypeExpression)expression);
				case (int)ServiceExpressionType.Comment:
					return this.VisitCommentExpression((CommentExpression)expression);
			}

			return base.Visit(expression);
		}

		protected virtual Expression VisitGroupedExpressionsExpression(GroupedExpressionsExpression expression)
		{
			var expressions = this.VisitExpressionList(expression.Expressions);

			if (expressions != expression.Expressions)
			{
				return new GroupedExpressionsExpression(expressions);
			}

			return expression;
		}

		protected virtual Expression VisitBlockExpression(CodeBlockExpression expression)
		{
			var expressions = this.VisitExpressionList(expression.Expressions);

			if (expressions != expression.Expressions)
			{
				return new CodeBlockExpression(expressions);
			}

			return expression;
		}

		protected virtual Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var expressions = this.VisitExpressionList(method.Parameters);

			if (expressions != method.Parameters)
			{
				return new MethodDefinitionExpression(method.Name, expressions, method.ReturnType);
			}

			return method;
		}

		protected virtual Expression VisitParameterDefinitionExpression(ParameterDefinitionExpression parameter)
		{
			return parameter;
		}

		protected virtual Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			return property;
		}

		protected virtual Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			Expression header = null, body = null;

			if (expression.Header != null)
			{
				header = this.Visit(expression.Header);
			}

			if (expression.Body != null)
			{
				body = this.Visit(expression.Body);
			}

			if (header != expression.Header || body != expression.Body)
			{
				return new TypeDefinitionExpression(header, body, expression.Name, expression.BaseType);
			}

			return expression;
		}

		protected virtual Expression VisitIncludeStatementExpresson(IncludeStatementExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitReferencedTypeExpresson(ReferencedTypeExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitCommentExpression(CommentExpression expression)
		{
			return expression;
		}
	}
}
