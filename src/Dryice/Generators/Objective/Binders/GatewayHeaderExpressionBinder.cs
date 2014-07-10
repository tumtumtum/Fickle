using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		private MethodDefinitionExpression CreateInitWithOptionsMethod()
		{
			return new MethodDefinitionExpression("initWithOptions", new Expression[] { DryExpression.Parameter("NSDictionary", "options") }.ToReadOnlyCollection(), DryType.Define("id"), null, true, null);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();

			var body = new Expression[]
			{
				Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement()
			};

			var newParameters = new List<Expression>(method.Parameters);
			newParameters.Add(DryExpression.Parameter(new DryDelegateType(typeof(void), new DryParameterInfo(method.ReturnType, "response")), "callback"));

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), Expression.Block(body), true, null);
		}
			
		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKTimeSpan.h"));
			}

			includeExpressions.Add(new IncludeStatementExpression("PKDictionarySerializable.h"));

			var referencedUserTypes = referencedTypes.Where
			(
				TypeSystem.IsNotPrimitiveType).Sorted((x, y) => 
				x.Name.Length == y.Name.Length ? String.CompareOrdinal(x.Name, y.Name) : x.Name.Length - y.Name.Length
			);

			includeExpressions.AddRange(referencedUserTypes.Select(type => new IncludeStatementExpression(type.Name + ".h")));
			includeExpressions.Add(new IncludeStatementExpression("PKWebServiceClient.h"));

			var optionsProperty = new PropertyDefinitionExpression("options", DryType.Define("NSDictionary"), true);

			var comment = new CommentExpression("This file is AUTO GENERATED");
			var header = new Expression[] { comment, includeExpressions.ToGroupedExpression() }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
			
			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				optionsProperty,
				this.CreateInitWithOptionsMethod(),
				this.Visit(expression.Body)
			);

			var interfaceTypes = new List<ServiceClass>();

			if (expression.InterfaceTypes != null)
			{
				interfaceTypes.AddRange(expression.InterfaceTypes);
			}

			interfaceTypes.Add(ServiceClass.Make("PKWebServiceClientDelegate"));

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, true, expression.Attributes, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
