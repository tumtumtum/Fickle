//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class PropertiesFromDictionaryExpressonBinder
		: ServiceExpressionVisitor
	{
		private readonly Type type;
		private readonly List<Expression> propertyGetterExpressions = new List<Expression>();

		protected PropertiesFromDictionaryExpressonBinder(Type type)
		{
			this.type = type;
		}

		public static Expression Bind(TypeDefinitionExpression expression)
		{
			var builder = new PropertiesFromDictionaryExpressonBinder(expression.Type);

			builder.Visit(expression);

			return builder.propertyGetterExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
		}

		internal static Expression GetDeserializeExpressionProcessValueDeserializer(Type valueType, Expression value, Func<Expression, Expression> processOutputValue)
		{
			Expression outputValue;
			var nsNull = DryExpression.StaticCall("NSNull", DryType.Define("id"), "null", null);

			if (TypeSystem.IsPrimitiveType(valueType))
			{
				ConditionalExpression ifExpression;
				var underlyingType = valueType.GetUnwrappedNullableType();

				if (underlyingType.IsNumericType() || underlyingType == typeof(bool))
				{
					var typeToCompare = new DryType("NSNumber");

					if (underlyingType.IsEnum && valueType.GetUnderlyingType() == null)
					{
						outputValue = Expression.Convert(DryExpression.Call(value, typeof(int), "intValue", null), valueType);
					}
					else
					{
						outputValue = Expression.Convert(value, valueType);
					}

					ifExpression = Expression.IfThen(Expression.TypeIs(value, typeToCompare), processOutputValue(outputValue).ToBlock());
				}
				else if (underlyingType.IsEnum)
				{
					Expression firstIf;

					if (valueType.IsNullable())
					{
						outputValue = Expression.Convert(value, valueType);

						firstIf = Expression.IfThen(Expression.TypeIs(value, DryType.Define("NSNumber")), processOutputValue(outputValue).ToBlock());
					}
					else
					{
						outputValue = Expression.Convert(DryExpression.Call(value, typeof(int), "intValue", null), valueType);
						
						firstIf = Expression.IfThen(Expression.TypeIs(value, DryType.Define("NSNumber")), processOutputValue(outputValue).ToBlock());
					}

					var parsedValue = DryExpression.Variable(underlyingType, "parsedValue");
					var success = Expression.Variable(typeof(bool), "success");

					var parameters = new[] { new DryParameterInfo(typeof(string), "value"), new DryParameterInfo(underlyingType, "outValue", true) };
					var methodInfo = new DryMethodInfo(null, typeof(bool), TypeSystem.GetPrimitiveName(underlyingType, true) + "TryParse", parameters, true);

					if (valueType.IsNullable())
					{
						outputValue = Expression.Convert(Expression.Condition(success, Expression.Convert(Expression.Convert(parsedValue, valueType), DryType.Define("id")), Expression.Convert(DryExpression.StaticCall(DryType.Define("NSNull"), valueType, "null", null), DryType.Define("id"))), valueType);
					}
					else
					{
						outputValue = Expression.Convert(parsedValue, valueType);
					}

					var secondIf = Expression.IfThenElse
					(
						Expression.TypeIs(value, DryType.Define("NSString")),
						DryExpression.Block
						(
							new ParameterExpression[]
							{
								parsedValue,
								success
							},
							Expression.IfThen
							(
								Expression.Not(Expression.Assign(success, Expression.Call(null, methodInfo, Expression.Convert(value, typeof(string)), parsedValue))),
								Expression.Assign(parsedValue, Expression.Convert(Expression.Constant(0), underlyingType)).ToStatement().ToBlock()
							),
							processOutputValue(outputValue)
						),
						firstIf
					);

					ifExpression = secondIf;
				}
				else
				{
					ifExpression = Expression.IfThen(Expression.TypeIs(value, typeof(string)), processOutputValue(Expression.Convert(Expression.Convert(value, typeof(object)), valueType)).ToBlock());
				}

				if (valueType.IsNullable())
				{
					ifExpression = Expression.IfThenElse
					(
						Expression.Equal(value, nsNull),
						processOutputValue(Expression.Convert(value, valueType)).ToBlock(),
						ifExpression
					);
				}

				return ifExpression;
			}
			else if (valueType is DryType && ((DryType)valueType).ServiceClass != null)
			{
				outputValue = DryExpression.New(valueType, "initWithPropertyDictionary", DryExpression.Convert(value, "NSDictionary"));

				return Expression.IfThen(Expression.TypeIs(value, DryType.Define("NSDictionary")), processOutputValue(outputValue).ToBlock());
			}
			else if (valueType is DryType && ((DryType)valueType).ServiceEnum != null)
			{
				return Expression.IfThen(Expression.TypeIs(value, DryType.Define("NSNumber")), processOutputValue(value).ToBlock());
			}
			else if (valueType is DryListType)
			{
				var listType = valueType as DryListType;

				var arrayVar = DryExpression.Variable("NSMutableArray", "array");
				var variables = new[] { arrayVar };
				var arrayItem = DryExpression.Parameter(DryType.Define("id"), "arrayItem");

				var forEachBody = GetDeserializeExpressionProcessValueDeserializer(listType.ListElementType, arrayItem, c => DryExpression.Call(arrayVar, "addObject", Expression.Convert(c, typeof(object))).ToStatement());
				
				var ifThen = Expression.IfThen
				(
					Expression.TypeIs(value, DryType.Define("NSArray")),
					DryExpression.Block
					(
						variables,
						Expression.Assign(arrayVar, DryExpression.New("NSMutableArray", "initWithCapacity", DryExpression.Call(value, typeof(int), "count", null))).ToStatement(),
						DryExpression.ForEach(arrayItem, value, DryExpression.Block(forEachBody)),
						processOutputValue(Expression.Convert(arrayVar, valueType))
					)
				);

				return ifThen;
			}
			else
			{
				throw new InvalidOperationException("Unsupported property type: " + valueType);
			}
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var dictionaryType = new DryType("NSDictionary");
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new DryMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new DryParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));
			var propertyExpression = Expression.Property(Expression.Parameter(this.type, "self"), new DryPropertyInfo(this.type, property.PropertyType, property.PropertyName.Uncapitalize()));
			
			var expressions = new List<Expression>
			{
				DryExpression.Comment(property.PropertyName),
				Expression.Assign(currentValueFromDictionary, objectForKeyCall).ToStatement(),
				GetDeserializeExpressionProcessValueDeserializer(property.PropertyType, currentValueFromDictionary, c => Expression.Assign(propertyExpression, c).ToStatement())
			};

			this.propertyGetterExpressions.Add(expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
