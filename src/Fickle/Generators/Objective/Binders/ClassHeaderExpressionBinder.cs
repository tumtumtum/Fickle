﻿//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.Objective.Binders
{
	public class ClassHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;

		private ClassHeaderExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new ClassHeaderExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.ToCamelCase();

			var propertyDefinition = new PropertyDefinitionExpression(name, property.PropertyType, true, property.Modifiers);

			if (name.StartsWith("new"))
			{
				var attributedPropertyGetter = new MethodDefinitionExpression(name, new Expression[0], property.PropertyType, Expression.Empty(), true, "(objc_method_family(none))", null);

				return new Expression[] { propertyDefinition, attributedPropertyGetter }.ToStatementisedGroupedExpression();
			}
			else
			{
				return propertyDefinition;
			}
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<IncludeExpression>();
			var importExpressions = new List<Expression>();
			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (!this.codeGenerationContext.Options.ImportDependenciesAsFramework)
			{
				if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
				{
					includeExpressions.Add(FickleExpression.Include("PKUUID.h"));
				}

				if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
				{
					includeExpressions.Add(FickleExpression.Include("PKTimeSpan.h"));
				}

				includeExpressions.Add(FickleExpression.Include("PKDictionarySerializable.h"));
				includeExpressions.Add(FickleExpression.Include("PKFormEncodingSerializable.h"));
			}
			else
			{
				importExpressions.Add(new CodeLiteralExpression(c => c.WriteLine("@import PlatformKit;")));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(FickleExpression.Include(expression.Type.BaseType.Name + ".h"));
			}

			includeExpressions.AddRange(referencedTypes.Where(c => c.IsEnum).Select(c => FickleExpression.Include(c.Name + ".h")));

			includeExpressions.Sort(IncludeExpression.Compare);

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerExpressions = new List<Expression>()
			{
				new[] { comment }.ToStatementisedGroupedExpression(),
				importExpressions.Count == 0 ? null : importExpressions.ToStatementisedGroupedExpression(),
				includeExpressions.ToStatementisedGroupedExpression()
			};
			
			var referencedTypeExpressions = referencedTypes
				.Where(ObjectiveBinderHelpers.TypeIsServiceClass)
				.Where(c => c != expression.Type.BaseType)
				.OrderBy(x => x.Name.Length)
				.ThenBy(x => x.Name)
				.Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			if (referencedTypeExpressions.Count > 0)
			{
				headerExpressions.Add(referencedTypeExpressions.ToStatementisedGroupedExpression());
			}

			var header = headerExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var propertyBody = this.Visit(expression.Body);

			var interfaceTypes = new List<Type>
			{
				FickleType.Define("NSCopying"),
				FickleType.Define("PKDictionarySerializable"),
				FickleType.Define("PKFormEncodingSerializable")
			};

			return new TypeDefinitionExpression(expression.Type, header, propertyBody, true, expression.Attributes, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
