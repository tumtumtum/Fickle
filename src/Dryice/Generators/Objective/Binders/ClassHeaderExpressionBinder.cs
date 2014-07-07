//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

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

				return new Expression[] { propertyDefinition, attributedPropertyGetter }.ToGroupedExpression();
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

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType).Select(c => c));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKUUID.h"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKTimeSpan.h"));
			}

			var referencedTypeExpressions = referencedTypes.Where(c => c is DryType && ((DryType)c).ServiceClass != null).Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new [] { comment }.ToGroupedExpression();
			var headerGroup = includeExpressions.ToGroupedExpression();
			var referencedGroup =referencedTypeExpressions.ToGroupedExpression();

			var header = GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, commentGroup, headerGroup, referencedGroup);

			var propertyBody = this.Visit(expression.Body);

			var interfaceTypes = new ReadOnlyCollection<ServiceClass>(new List<ServiceClass>
			{
				new ServiceClass()
				{
					Name = "NSCopying"
				}
			});

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, propertyBody, true, null, interfaceTypes);
		}
	}
}
