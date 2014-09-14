//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
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
				includeExpressions.Add(FickleExpression.Include("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(FickleExpression.Include("PKTimeSpan.h"));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(FickleExpression.Include(expression.Type.BaseType.Name + ".h"));
			}

			includeExpressions.Add(FickleExpression.Include("PKDictionarySerializable.h"));
			includeExpressions.AddRange(referencedTypes.Where(c => c.IsEnum).Select(c => (Expression)FickleExpression.Include(c.Name + ".h")));

			includeExpressions = includeExpressions.Select(c => (IncludeExpression)c).OrderBy(c => c.FileName.Length).Select(c => (Expression)c).ToList();

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new [] { comment }.ToStatementisedGroupedExpression();
			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();

			var headerExpressions = new List<Expression>
			{
				commentGroup,
				headerGroup
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
				FickleType.Define("PKDictionarySerializable")
			};

			return new TypeDefinitionExpression(expression.Type, header, propertyBody, true, null, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
