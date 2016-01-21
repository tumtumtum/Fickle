using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
{
	public class GatewayHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly List<MethodDefinitionExpression> methods;
		public CodeGenerationContext CodeGenerationContext { get; private set; }

		private GatewayHeaderExpressionBinder(CodeGenerationContext codeGenerationContext, List<MethodDefinitionExpression> methods)
		{
			this.methods = methods;
			this.CodeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression, List<MethodDefinitionExpression> methods)
		{
			var binder = new GatewayHeaderExpressionBinder(codeGenerationContext, methods);

			return binder.Visit(expression);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<IncludeExpression>();
			var importExpressions = new List<Expression>();

			var optionsProperty = new PropertyDefinitionExpression("options", FickleType.Define("NSDictionary"), true);
			var responseFilterProperty = new PropertyDefinitionExpression("responseFilter", FickleType.Define("FKGatewayResponseFilter", isInterface:true), true, new[] { "weak" });

			var body = FickleExpression.GroupedWide
			(
				optionsProperty,
				responseFilterProperty,
                new GroupedExpressionsExpression(methods.Select(c => c.ChangePredeclaration(true)))
			);

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (!this.CodeGenerationContext.Options.ImportDependenciesAsFramework)
			{
				if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
				{
					includeExpressions.Add(FickleExpression.Include("PKUUID.h"));
				}

				if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
				{
					includeExpressions.Add(FickleExpression.Include("PKTimeSpan.h"));
				}

				includeExpressions.Add(FickleExpression.Include("PKWebServiceClient.h"));
				includeExpressions.Add(FickleExpression.Include("PKDictionarySerializable.h"));
			}
			else
			{
				importExpressions.Add(new CodeLiteralExpression(c => c.WriteLine("@import PlatformKit;")));
			}

			includeExpressions.Add(FickleExpression.Include("FKGatewayResponseFilter.h"));

			var referencedUserTypes = referencedTypes
				.Where(c => (c is FickleType && ((FickleType)c).ServiceClass != null) || c is FickleType && ((FickleType)c).ServiceEnum != null)
				.Sorted((x, y) => x.Name.Length == y.Name.Length ? String.CompareOrdinal(x.Name, y.Name) : x.Name.Length - y.Name.Length);

			includeExpressions.AddRange(referencedUserTypes.Select(c => FickleExpression.Include(c.Name + ".h")));
			
			var comment = new CommentExpression("This file is AUTO GENERATED");

			var header = new Expression[]
			{
				comment,
				importExpressions.Count == 0 ? null : importExpressions.ToStatementisedGroupedExpression(),
				includeExpressions.Sorted(IncludeExpression.Compare).ToStatementisedGroupedExpression()
			}.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
			
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
