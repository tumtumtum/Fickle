//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Dryice.Expressions;

namespace Dryice
{
	public class ServiceExpressionVisitor
		: ExpressionVisitor
	{
		protected override Expression VisitExtension(Expression expression)
		{
			switch ((int)expression.NodeType)
			{
				case (int)ServiceExpressionType.ForEach:
					return this.VisitForEachExpression((ForEachExpression)expression);
				case (int)ServiceExpressionType.Statement:
					return this.VisitStatementExpression((StatementExpression)expression);
				case (int)ServiceExpressionType.GroupedExpressions:
					return this.VisitGroupedExpressionsExpression((GroupedExpressionsExpression)expression);
				case (int)ServiceExpressionType.MethodDefinition:
					return this.VisitMethodDefinitionExpression((MethodDefinitionExpression)expression);
				case (int)ServiceExpressionType.PropertyDefinition:
					return this.VisitPropertyDefinitionExpression((PropertyDefinitionExpression)expression);
				case (int)ServiceExpressionType.TypeDefinition:
					return this.VisitTypeDefinitionExpression((TypeDefinitionExpression)expression);
				case (int)ServiceExpressionType.IncludeStatement:
					return this.VisitIncludeStatementExpresson((IncludeExpression)expression);
				case (int)ServiceExpressionType.ReferencedType:
					return this.VisitReferencedTypeExpresson((ReferencedTypeExpression)expression);
				case (int)ServiceExpressionType.Comment:
					return this.VisitCommentExpression((CommentExpression)expression);
				case (int)ServiceExpressionType.CodeLiteral:
					return this.VisitCodeLiteralExpression((CodeLiteralExpression)expression);
				case (int)ServiceExpressionType.SimpleLambdaExpression:
					return this.VisitSimpleLambdaExpression((SimpleLambdaExpression)expression);
				case (int)ServiceExpressionType.FieldDefinition:
					return this.VisitFieldDefinitionExpression((FieldDefinitionExpression)expression);
			}

			return base.Visit(expression);
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			return this.Visit(original);
		}

		protected virtual Expression VisitGroupedExpressionsExpression(GroupedExpressionsExpression expression)
		{
			var expressions = this.VisitExpressionList(expression.Expressions);

			if (expressions != expression.Expressions)
			{
				return new GroupedExpressionsExpression(expressions, expression.Style);
			}

			return expression;
		}

		protected virtual Expression VisitStatementExpression(StatementExpression expression)
		{
			var ex = this.Visit(expression.Expression);

			if (ex != expression.Expression)
			{
				return ex.ToStatement();
			}

			return expression;
		}

		protected virtual Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var expressions = this.VisitExpressionList(method.Parameters);
			var body = this.Visit(method.Body);

			if (expressions != method.Parameters || body != method.Body)
			{
				return new MethodDefinitionExpression(method.Name, expressions, method.ReturnType, body, method.IsPredeclaration, method.RawAttributes);
			}

			return method;
		}

		protected virtual Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			return property;
		}

		protected virtual Expression VisitFieldDefinitionExpression(FieldDefinitionExpression field)
		{
			return field;
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
				return new TypeDefinitionExpression(expression.Type, header, body, expression.IsPredeclaration, null, expression.InterfaceTypes);
			}

			return expression;
		}

		protected virtual Expression VisitIncludeStatementExpresson(IncludeExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitReferencedTypeExpresson(ReferencedTypeExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitForEachExpression(ForEachExpression expression)
		{
			var parameter = (ParameterExpression)this.Visit(expression.VariableExpression);
			var target = this.Visit(expression.Target);
			var body = this.Visit(expression.Body);
			
			if (parameter != expression.VariableExpression || target != expression.Target || body != expression.Body)
			{
				return new ForEachExpression(parameter, target, body);
			}

			return expression;
		}

		protected virtual Expression VisitCommentExpression(CommentExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitCodeLiteralExpression(CodeLiteralExpression expression)
		{
			return expression;
		}

		protected virtual Expression VisitSimpleLambdaExpression(SimpleLambdaExpression expression)
		{
			var body = this.Visit(expression.Body);
			var parameters = this.VisitExpressionList(expression.Parameters);

			if (body != expression.Body || parameters != expression.Parameters)
			{
				return new SimpleLambdaExpression(expression.ReturnType, body, expression.Variables, parameters);
			}

			return expression;
		}
	}
}
