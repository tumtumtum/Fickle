//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class ClassHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		public static Expression Bind(Expression expression)
		{
			var binder = new ClassHeaderExpressionBinder();

			return binder.Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.Uncapitalize();

			var propertyDefinition = new PropertyDefinitionExpression(name, property.PropertyType, true);

			if (name.StartsWith("new"))
			{
				var attributedPropertyGetter = new MethodDefinitionExpression(name, null, property.PropertyType, null, true, "(objc_method_family(none))", null);

				return new Expression[] { propertyDefinition, attributedPropertyGetter }.ToStatementisedGroupedExpression();
			}
			else
			{
				return propertyDefinition;
			}
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(DryExpression.Include("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(DryExpression.Include("PKTimeSpan.h"));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(DryExpression.Include(expression.Type.BaseType.Name + ".h"));
			}

			includeExpressions.Add(DryExpression.Include("PKDictionarySerializable.h"));

			var referencedTypeExpressions = referencedTypes
				.Where(ObjectiveBinderHelpers.TypeIsServiceClass)
				.Where(c => c != expression.Type.BaseType)
				.Select(c => (Expression)new ReferencedTypeExpression(c));

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new [] { comment }.ToStatementisedGroupedExpression();
			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var referencedGroup =referencedTypeExpressions.ToStatementisedGroupedExpression();

			var header = new[] { commentGroup, headerGroup, referencedGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var propertyBody = this.Visit(expression.Body);

			var interfaceTypes = new List<Type>
			{
				DryType.Define("NSCopying"),
				DryType.Define("PKDictionarySerializable")
			};

			return new TypeDefinitionExpression(expression.Type, header, propertyBody, true, null, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
