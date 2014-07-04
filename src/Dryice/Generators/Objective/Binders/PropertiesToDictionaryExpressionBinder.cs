﻿//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dryice.Expressions;

namespace Dryice.Generators.Objective.Binders
{
	public class PropertiesToDictionaryExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly Type type;
		private readonly List<Expression> propertySetterExpressions = new List<Expression>();
 
		protected PropertiesToDictionaryExpressionBinder(Type type)
		{
			this.type = type;
		}

		public static Expression Build(TypeDefinitionExpression expression)
		{
			var builder = new PropertiesToDictionaryExpressionBinder(expression.Type);

			builder.Visit(expression);

			return builder.propertySetterExpressions.ToGroupedExpression();
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Variable(this.type, "self");
			var retval = DryExpression.Variable("NSDictionary", "retval");
			var propertyExpression = DryExpression.Property(self, this.type, property.PropertyName);

			var setObjectForKeyMethodCall = DryExpression.MakeMethodCall(retval, "setObject", new
			{
				obj = Expression.Convert(propertyExpression, typeof(object)),
				forKey = Expression.Constant(property.PropertyName)
			});

			Expression setExpression = setObjectForKeyMethodCall.ToStatement();

			if (!property.PropertyType.IsPrimitive)
			{
				setExpression = Expression.IfThen(Expression.ReferenceNotEqual(Expression.Convert(propertyExpression, typeof(object)), Expression.Constant(null)), Expression.Block(setExpression));
			}

			var expressions = new List<Expression>()
			{
				DryExpression.Comment(property.PropertyName),
				setExpression
			};

			this.propertySetterExpressions.Add(expressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
