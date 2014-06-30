//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Platform;
using Dryice.Expressions;

namespace Dryice.Generators.Objective
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
			var comment = new CommentExpression(property.PropertyName);
			var expressions = new List<Expression>();

			var propertyType = property.PropertyType;
			var dictionaryType = new DryiceType("NSDictionary"); 
			var retval = Expression.Parameter(dictionaryType, "retval");
			var setObjectForKeyMethod = dictionaryType.GetMethod("setObject", typeof(void), new ParameterInfo[] { new DryParameterInfo(typeof(object), "obj"),new DryParameterInfo(typeof(string), "forKey") });
			var propertyExpression = Expression.Property(Expression.Parameter(type, "self"), new DryPropertyInfo(type, property.PropertyType, property.PropertyName));

			var setObjectForKeyMethodCall = Expression.Call(retval, setObjectForKeyMethod, Expression.Convert(propertyExpression, typeof(object)), Expression.Constant(property.PropertyName));

			Expression setExpression = new StatementExpression(setObjectForKeyMethodCall);

			if (!propertyType.IsPrimitive)
			{
				setExpression = Expression.IfThen(Expression.ReferenceNotEqual(Expression.Convert(propertyExpression, typeof(object)), Expression.Constant(null)), Expression.Block(setExpression));
			}

			expressions.Add(comment);
			expressions.Add(setExpression);

			propertySetterExpressions.Add(expressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
