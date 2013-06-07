//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Platform;
using Sublimate.Expressions;
using Sublimate.Model;

namespace Sublimate.Generators.Objective
{
	public class ObjectiveHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		public static Expression Bind(Expression expression)
		{
			var binder = new ObjectiveHeaderExpressionBinder();

			return binder.Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.Uncapitalize();

			var propertyDefinition = new PropertyDefinitionExpression(name, property.PropertyType, true);

			if (name.StartsWith("new"))
			{
				var attributedPropertyGetter = new MethodDefinitionExpression(name, null, property.PropertyType, true, "(objc_method_family(none))");

				return new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new List<Expression> { propertyDefinition, attributedPropertyGetter }));
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

			var lookup = new HashSet<PrimitiveType>(referencedTypes.Where(c => c.PrimitiveType.HasValue).Select(c => c.PrimitiveType.Value));

			if (lookup.Contains(PrimitiveType.Guid))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKUUID.h"));
			}

			if (lookup.Contains(PrimitiveType.TimeSpan))
			{
				includeExpressions.Add(new IncludeStatementExpression("PKTimeSpan.h"));
			}

			var referencedTypeExpressions = referencedTypes.Where(c => !c.IsPrimitive).Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new List<Expression> { comment }), true);
			var headerGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(includeExpressions), true);
			var referencedGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(referencedTypeExpressions), true);

			var header = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new [] { commentGroup, headerGroup, referencedGroup }));

			var propertyBody = this.Visit(expression.Body);

			var body = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new List<Expression> { propertyBody }));

			var interfaceTypes = new ReadOnlyCollection<ServiceType>(new List<ServiceType>
			{
				new ServiceType()
				{
					Name = "NSCopying"
				}
			});

			return new TypeDefinitionExpression(header, body, expression.Name, expression.BaseType, true, interfaceTypes);
		}
	}
}
