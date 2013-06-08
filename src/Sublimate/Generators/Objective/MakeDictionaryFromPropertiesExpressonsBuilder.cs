//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Platform;
using Sublimate.Expressions;

namespace Sublimate.Generators.Objective
{
	public class MakeDictionaryFromPropertiesExpressonsBuilder
		: ServiceExpressionVisitor
	{
		private readonly Type type;
		private readonly List<Expression> propertySetterExpressions = new List<Expression>();
 
		protected MakeDictionaryFromPropertiesExpressonsBuilder(Type type)
		{
			this.type = type;
		}

		public static Expression Build(TypeDefinitionExpression expression)
		{
			var builder = new MakeDictionaryFromPropertiesExpressonsBuilder(expression.Type);

			builder.Visit(expression);

			return new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(builder.propertySetterExpressions.ToArray()));
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var comment = new CommentExpression(property.PropertyName);
			var expressions = new List<Expression>();

			var propertyType = property.PropertyType;
			var dictionaryType = new SublimateType("NSDictionary"); 
			var retval = Expression.Parameter(dictionaryType, "retval");
			var setObjectForKeyMethod = dictionaryType.GetMethod("setObject", typeof(void), new ParameterInfo[] { new SublimateParameterInfo(typeof(object), "obj"),new SublimateParameterInfo(typeof(string), "forKey") });
			var propertyExpression = Expression.Property(Expression.Parameter(type, "self"), new SublimatePropertyInfo(type, property.PropertyType, property.PropertyName.Uncapitalize()));

			var setObjectForKeyMethodCall = Expression.Call(retval, setObjectForKeyMethod, Expression.Convert(propertyExpression, typeof(object)), Expression.Constant(property.PropertyName));

			Expression setExpression = new StatementsExpression(setObjectForKeyMethodCall);

			if (!propertyType.IsPrimitive)
			{
				setExpression = Expression.IfThen(Expression.ReferenceNotEqual(Expression.Convert(propertyExpression, typeof(object)), Expression.Constant(null)), Expression.Block(setExpression));
			}
			
			expressions.Add(new GroupedExpressionsExpression(comment, true));
			expressions.Add(setExpression);
			
			propertySetterExpressions.Add(new GroupedExpressionsExpression(expressions, false));

			return property;
		}
	}
}
