//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Objective.Binders
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
			var nsNull = DryExpression.StaticCall("NSNull", "id", "null", null);

			if (valueType.GetUnwrappedNullableType().IsEnum)
			{
				expression = value;

				if (options.SerializeEnumsAsStrings)
				{
					expression = DryExpression.Call(Expression.Convert(expression, valueType), typeof(string), "ToString", null);
				}

				expression = processOutputValue(expression);
			}
			else if (valueType.GetUnwrappedNullableType() == typeof(Guid))
			{
				expression = DryExpression.Call(Expression.Convert(value, valueType), typeof(string), "ToString", null);
				
				expression = processOutputValue(expression);
			}
			else if (valueType is DryListType)
			{
				var listType = valueType as DryListType;

				if (listType.ListElementType.GetUnwrappedNullableType().IsNumericType()
				    || (listType.ListElementType.GetUnwrappedNullableType().IsEnum && !options.SerializeEnumsAsStrings))
				{
					return processOutputValue(value);
				}
				else
				{
					var arrayVar = DryExpression.Variable("NSMutableArray", "array");
					var variables = new[] { arrayVar };
					var arrayItem = DryExpression.Parameter(DryType.Define("id"), "arrayItem");

					var supportsNull = listType.ListElementType.IsNullable()
						|| !listType.ListElementType.IsValueType;

					var forEachBody = Expression.IfThenElse
					(
						Expression.ReferenceEqual(arrayItem, nsNull),
						supportsNull ? DryExpression.Call(arrayVar, "addObject", Expression.Convert(nsNull, typeof(object))).ToStatement().ToBlock() : Expression.Continue(Expression.Label()).ToStatement().ToBlock(),
						GetSerializeExpression(listType.ListElementType, arrayItem, options, true, c => DryExpression.Call(arrayVar, "addObject", Expression.Convert(c, typeof(object))).ToStatement()).ToBlock()
					);

					expression = DryExpression.Block
					(
						variables,
						Expression.Assign(arrayVar, DryExpression.New("NSMutableArray", "initWithCapacity", DryExpression.Call(value, typeof(int), "count", null))).ToStatement(),
						DryExpression.ForEach(arrayItem, value, DryExpression.Block(forEachBody)),
						processOutputValue(Expression.Convert(arrayVar, valueType))
					);
				}
			}
			else if (valueType.IsServiceType())
			{
				expression = processOutputValue(DryExpression.Call(value, value.Type, "allPropertiesAsDictionary", null));
			}
			else
			{
				expression = processOutputValue(value);
			}

			if (!skipIfNull)
			{
				if (!TypeSystem.IsPrimitiveType(valueType) || valueType.IsNullable())
				{
					if (value.Type == DryType.Define("id") || value.Type == typeof(object))
					{
						expression = Expression.IfThen
						(
							Expression.And
							(
								Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), Expression.Constant(null)),
								Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), nsNull)
							),
							expression is BlockExpression ? expression : DryExpression.Block(expression)
						);
					}
					else
					{
						expression = Expression.IfThen
						(
							Expression.ReferenceNotEqual(Expression.Convert(value, typeof(object)), Expression.Constant(null)),
							expression is BlockExpression ? expression : DryExpression.Block(expression)
						);
					}
				}
			}

			return expression;
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Variable(this.type, "self");
			var retval = DryExpression.Variable("NSDictionary", "retval");
			var propertyExpression = DryExpression.Property(self, property.PropertyType, property.PropertyName);

			var expression = GetSerializeExpression(property.PropertyType, propertyExpression, context.Options, false, c => DryExpression.Call(retval, "setObject", new
			{
				obj = Expression.Convert(c, typeof(object)),
				forKey = Expression.Constant(property.PropertyName)
			}).ToStatement());

			this.propertySetterExpressions.Add(new [] { DryExpression.Comment(property.PropertyName), expression }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
