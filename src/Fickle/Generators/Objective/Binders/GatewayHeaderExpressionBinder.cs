using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Fickle.Model;
using Platform;

namespace Fickle.Generators.Objective.Binders
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
			return new MethodDefinitionExpression("initWithOptions", new Expression[] { FickleExpression.Parameter("NSDictionary", "options") }.ToReadOnlyCollection(), FickleType.Define("id"), null, true, null);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.ToCamelCase();

			var body = new Expression[]
			{
				Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement()
			};

			var responseType = ObjectiveBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);

			var newParameters = new List<Expression>(method.Parameters)
			{
				FickleExpression.Parameter(new FickleDelegateType(typeof(void), new FickleParameterInfo(responseType, "response")), "callback")
			};

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), Expression.Block(body), true, null);
		}
			
		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<IncludeExpression>();
			var optionsProperty = new PropertyDefinitionExpression("options", FickleType.Define("NSDictionary"), true);

			var body = FickleExpression.GroupedWide
			(
				optionsProperty,
				FickleExpression.Grouped
				(
					this.CreateInitWithOptionsMethod(),
					this.Visit(expression.Body)
				)
			);

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(FickleExpression.Include("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(FickleExpression.Include("PKTimeSpan.h"));
			}

			includeExpressions.Add(FickleExpression.Include("PKDictionarySerializable.h"));
			includeExpressions.Add(FickleExpression.Include("PKWebServiceClient.h"));

			var referencedUserTypes = referencedTypes
				.Where(c => (c is FickleType && ((FickleType)c).ServiceClass != null) || c is FickleType && ((FickleType)c).ServiceEnum != null)
				.Sorted((x, y) => x.Name.Length == y.Name.Length ? String.CompareOrdinal(x.Name, y.Name) : x.Name.Length - y.Name.Length);

			includeExpressions.AddRange(referencedUserTypes.Select(c => FickleExpression.Include(c.Name + ".h")));
			
			var comment = new CommentExpression("This file is AUTO GENERATED");
			var header = new Expression[] { comment, includeExpressions.Sorted(IncludeExpression.Compare).ToStatementisedGroupedExpression() }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
			
			var interfaceTypes = new List<Type>();

			if (expression.InterfaceTypes != null)
			{
				interfaceTypes.AddRange(expression.InterfaceTypes);
			}

			interfaceTypes.Add(FickleType.Define("PKWebServiceClientDelegate"));

			return new TypeDefinitionExpression(expression.Type, header, body, true, expression.Attributes, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
