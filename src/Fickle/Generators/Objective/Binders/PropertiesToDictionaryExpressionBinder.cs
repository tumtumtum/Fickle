//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
{
	public class PropertiesToDictionaryExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly Type type;
		private readonly CodeGenerationContext context;
		private readonly List<Expression> propertySetterExpressions = new List<Expression>();

		protected PropertiesToDictionaryExpressionBinder(Type type, CodeGenerationContext context)
		{
			this.type = type;
			this.context = context;
		}

		public static Expression Build(TypeDefinitionExpression expression, CodeGenerationContext context)
		{
			var builder = new PropertiesToDictionaryExpressionBinder(expression.Type, context);

			builder.Visit(expression);

			return builder.propertySetterExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
		}

		internal static Expression GetSerializeExpression(Type valueType, Expression value, CodeGenerationOptions options, bool skipIfNull, Func<Expression, Expression> processOutputValue)
		{
			Expression expression;
			var nsNull = FickleExpression.StaticCall("NSNull", "id", "null", null);

			if (valueType.GetUnwrappedNullableType().IsEnum)
			{
				expression = value;

				if (options.SerializeEnumsAsStrings)
				{
					expression = FickleExpression.Call(Expression.Convert(expression, valueType), typeof(string), "ToString", null);
				}

				expression = processOutputValue(expression);
			}
			else if (valueType.GetUnwrappedNullableType() == typeof(Guid))
			{
				expression = FickleExpression.Call(Expression.Convert(value, valueType), typeof(string), "ToString", null);
				
				expression = processOutputValue(expression);
			}
			else if (valueType is FickleListType)
			{
				var listType = valueType as FickleListType;

				if (listType.ListElementType.GetUnwrappedNullableType().IsNumericType()
				    || (listType.ListElementType.GetUnwrappedNullableType().IsEnum && !options.SerializeEnumsAsStrings))
				{
					return processOutputValue(value);
				}
				else
				{
					var arrayVar = FickleExpression.Variable("NSMutableArray", "array");
					var variables = new[] { arrayVar };
					var arrayItem = FickleExpression.Parameter(FickleType.Define("id"), "arrayItem");

					var supportsNull = listType.ListElementType.IsNullable()
						|| !listType.ListElementType.IsValueType;

					var forEachBody = Expression.IfThenElse
					(
						Expression.ReferenceEqual(arrayItem, nsNull),
						supportsNull ? FickleExpression.Call(arrayVar, "addObject", Expression.Convert(nsNull, typeof(object))).ToStatement().ToBlock() : Expression.Continue(Expression.Label()).ToStatement().ToBlock(),
						GetSerializeExpression(listType.ListElementType, arrayItem, options, true, c => FickleExpression.Call(arrayVar, "addObject", Expression.Convert(c, typeof(object))).ToStatement()).ToBlock()
					);

					expression = FickleExpression.Block
					(
						variables,
						Expression.Assign(arrayVar, FickleExpression.New("NSMutableArray", "initWithCapacity", FickleExpression.Call(value, typeof(int), "count", null))).ToStatement(),
						FickleExpression.ForEach(arrayItem, value, FickleExpression.Block(forEachBody)),
						processOutputValue(Expression.Convert(arrayVar, valueType))
					);
				}
			}
			else if (valueType.IsServiceType())
			{
				expression = processOutputValue(FickleExpression.Call(value, value.Type, "allPropertiesAsDictionary", null));
			}
			else
			{
				expression = processOutputValue(value);
			}

			if (!skipIfNull)
			{
				if (!TypeSystem.IsPrimitiveType(valueType) || valueType.IsNullable() || valueType.IsClass)
				{
					if (value.Type == FickleType.Define("id") || value.Type == typeof(object))
					{
						expression = Expression.IfThen
						(
							Expression.And
							(
								Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), Expression.Constant(null)),
								Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), nsNull)
							),
							expression is BlockExpression ? expression : FickleExpression.Block(expression)
						);
					}
					else
					{
						expression = Expression.IfThen
						(
							Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), Expression.Constant(null)),
							expression is BlockExpression ? expression : FickleExpression.Block(expression)
						);
					}
				}
			}

			return expression;
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Variable(this.type, "self");
			var retval = FickleExpression.Variable("NSDictionary", "retval");
			var propertyExpression = FickleExpression.Property(self, property.PropertyType, property.PropertyName);

			var expression = GetSerializeExpression(property.PropertyType, propertyExpression, context.Options, false, c => FickleExpression.Call(retval, "setObject", new
			{
				obj = Expression.Convert(c, typeof(object)),
				forKey = Expression.Constant(property.PropertyName)
			}).ToStatement());

			this.propertySetterExpressions.Add(new [] { FickleExpression.Comment(property.PropertyName), expression }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
