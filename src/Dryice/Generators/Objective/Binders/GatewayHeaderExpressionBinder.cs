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
		public CodeGenerationContext CodeGenerationContext { get; private set; }

		private GatewayHeaderExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new GatewayHeaderExpressionBinder(codeGenerationContext);

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

			var responseType = ObjectiveBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);

			var newParameters = new List<Expression>(method.Parameters)
			{
				DryExpression.Parameter(new DryDelegateType(typeof(void), new DryParameterInfo(responseType, "response")), "callback")
			};

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), Expression.Block(body), true, null);
		}
			
		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>();
			var optionsProperty = new PropertyDefinitionExpression("options", DryType.Define("NSDictionary"), true);

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				optionsProperty,
				this.CreateInitWithOptionsMethod(),
				this.Visit(expression.Body)
			);

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body);
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
			includeExpressions.Add(new IncludeStatementExpression("PKWebServiceClient.h"));

			var referencedUserTypes = referencedTypes
				.Where(c => c is DryType && ((DryType)c).ServiceClass != null)
				.Sorted((x, y) => x.Name.Length == y.Name.Length ? String.CompareOrdinal(x.Name, y.Name) : x.Name.Length - y.Name.Length);

			includeExpressions.AddRange(referencedUserTypes.Select(c => new IncludeStatementExpression(c.Name + ".h")));
			
			var comment = new CommentExpression("This file is AUTO GENERATED");
			var header = new Expression[] { comment, includeExpressions.ToGroupedExpression() }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
			
			var interfaceTypes = new List<Type>();

			if (expression.InterfaceTypes != null)
			{
				interfaceTypes.AddRange(expression.InterfaceTypes);
			}

			interfaceTypes.Add(DryType.Define("PKWebServiceClientDelegate"));

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, true, expression.Attributes, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
