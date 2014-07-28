//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dryice.Expressions;
using Dryice.Generators.Objective;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java.Binders
{
	public class ClassExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		private Type currentType;
		
		private ClassExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new ClassExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.Uncapitalize();

			return new PropertyDefinitionExpression(name, property.PropertyType, true);
		}

		protected virtual MethodDefinitionExpression CreateCreateErrorResponseMethod()
		{
			var errorCode = Expression.Parameter(typeof(string), "errorCode");
			var message = Expression.Parameter(typeof(string), "errorMessage");
			var stackTrace = Expression.Parameter(typeof(string), "stackTrace");

			var parameters = new Expression[]
			{
				errorCode,
				message,
				stackTrace
			};

			var response = DryExpression.Variable(DryType.Define("id"), "response");
			var responseStatus = DryExpression.Call(response, "ResponseStatus", "responseStatus", null);
			var newResponseStatus = DryExpression.New("ResponseStatus", "init", null);

			var body = DryExpression.Block
			(
				new[] { response },
				Expression.IfThen(Expression.IsTrue(Expression.Equal(responseStatus, Expression.Constant(null, responseStatus.Type))), DryExpression.Block(DryExpression.Call(response, "setResponseStatus", newResponseStatus))),
				DryExpression.Call(responseStatus, typeof(string), "setErrorCode", errorCode),
				DryExpression.Call(responseStatus, typeof(string), "setMessage", message),
				Expression.Return(Expression.Label(), response)
			);

			return new MethodDefinitionExpression("createErrorResponse", new ReadOnlyCollection<Expression>(parameters), AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentType = expression.Type;
			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var includeExpressions = referencedTypes
				.Where(ObjectiveBinderHelpers.TypeIsServiceClass)
				.Where(c => c != expression.Type.BaseType)
				.Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			foreach (var referencedType in referencedTypes.Where(JavaBinderHelpers.TypeIsServiceClass))
			{
				includeExpressions.Add(DryExpression.Include(referencedType.Name));
			}

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.UUID"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.Date"));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(DryExpression.Include(expression.Type.BaseType.Name));
			}

			includeExpressions.Add(DryExpression.Include("java.util.Dictionary"));

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var methods = new List<Expression>
			{
				this.Visit(expression.Body),
				CreateCreateErrorResponseMethod()
			};

			var body = methods.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false, null, null);
		}
	}
}
