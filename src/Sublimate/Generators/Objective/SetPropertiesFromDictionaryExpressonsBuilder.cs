//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Platform;
using Sublimate.Expressions;

namespace Sublimate.Generators.Objective
{
	public class SetPropertiesFromDictionaryExpressonsBuilder
		: ServiceExpressionVisitor
	{
		private readonly Type type;
		private readonly List<Expression> propertyGetterExpressions = new List<Expression>();

		protected SetPropertiesFromDictionaryExpressonsBuilder(Type type)
		{
			this.type = type;
		}

		public static Expression Build(TypeDefinitionExpression expression)
		{
			var builder = new SetPropertiesFromDictionaryExpressonsBuilder(expression.Type);

			builder.Visit(expression);

			return new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(builder.propertyGetterExpressions.ToArray()));
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var comment = new CommentExpression(property.PropertyName);
			var expressions = new List<Expression>();

			var dictionaryType = new SublimateType("NSDictionary"); 
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new SublimateMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new SublimateParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));

			var propertyExpression = Expression.Property(Expression.Parameter(type, "self"), new SublimatePropertyInfo(type, property.PropertyType, property.PropertyName.Uncapitalize()));

			Type typeToCompare;
			Expression assignmentValue = null;

			if (TypeSystem.IsPrimitiveType(property.PropertyType))
			{
				var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

				if (underlyingType == typeof(byte) || underlyingType == typeof(char) || underlyingType == typeof(short)
					|| underlyingType == typeof(int) || underlyingType == typeof(long))
				{
					typeToCompare = new SublimateType("NSNumber");
				}
				else
				{
					typeToCompare = typeof(string);
				}

				assignmentValue = Expression.Convert(currentValueFromDictionary, propertyExpression.Type);
			}
			else if (property.PropertyType is SublimateType && ((SublimateType)property.PropertyType).ServiceType != null)
			{
				typeToCompare = dictionaryType;

				assignmentValue = Expression.New(((SublimateType)property.PropertyType).GetConstructor("initWithPropertyDictionary", dictionaryType), Expression.Convert(currentValueFromDictionary, dictionaryType));
			}
			else
			{
				throw new InvalidOperationException("Unsupported property type: " + property.PropertyType);
			}

			var assignmentExpression = Expression.Assign(propertyExpression, assignmentValue);

			expressions.Add(new GroupedExpressionsExpression(comment, true));
			expressions.Add(new StatementsExpression(Expression.Assign(currentValueFromDictionary, objectForKeyCall)));
			expressions.Add(Expression.IfThen(Expression.TypeIs(currentValueFromDictionary, typeToCompare), Expression.Block(new StatementsExpression(assignmentExpression))));

			this.propertyGetterExpressions.Add(new GroupedExpressionsExpression(expressions));

			return property;
		}
	}
}
