using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class GatewayHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly ServiceModel serviceModel;

		private GatewayHeaderExpressionBinder(ServiceModel serviceModel)
		{
			this.serviceModel = serviceModel;
		}

		public static Expression Bind(ServiceModel serviceModel, Expression expression)
		{
			var binder = new GatewayHeaderExpressionBinder(serviceModel);

			return binder.Visit(expression);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();

			var body = new Expression[]
			{
				Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement()
			};

			return new MethodDefinitionExpression(methodName, method.Parameters, method.ReturnType, Expression.Block(body), true, null);
		}
			
		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType).Select(c => c));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKTimeSpan.h"));
			}

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var body = GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, this.Visit(expression.Body));

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, true, null);
		}
	}
}
