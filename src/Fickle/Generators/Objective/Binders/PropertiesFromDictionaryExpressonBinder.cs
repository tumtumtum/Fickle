//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
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

		public static Expression Bind(TypeDefinitionExpression expression, out int count)
		{
			var builder = new PropertiesFromDictionaryExpressonBinder(expression.Type);

			builder.Visit(expression);

			count = builder.propertyGetterExpressions.Count;

			return builder.propertyGetterExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
		}

		internal static Expression GetDeserializeExpressionProcessValueDeserializer(Type valueType, Expression value, Func<Expression, Expression> processOutputValue)
		{
			Expression outputValue;
			var nsNull = FickleExpression.StaticCall("NSNull", FickleType.Define("id"), "null", null);

			if (TypeSystem.IsPrimitiveType(valueType))
			{
				ConditionalExpression ifExpression;
				var underlyingType = valueType.GetUnwrappedNullableType();

				if (underlyingType.IsNumericType() || underlyingType == typeof(bool))
				{
					var typeToCompare = new FickleType("NSNumber");

					if (underlyingType.IsEnum && valueType.GetUnderlyingType() == null)
					{
						outputValue = Expression.Convert(FickleExpression.Call(value, typeof(int), "intValue", null), valueType);
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

						firstIf = Expression.IfThen(Expression.TypeIs(value, FickleType.Define("NSNumber")), processOutputValue(outputValue).ToBlock());
					}
					else
					{
						outputValue = Expression.Convert(FickleExpression.Call(value, typeof(int), "intValue", null), valueType);
						
						firstIf = Expression.IfThen(Expression.TypeIs(value, FickleType.Define("NSNumber")), processOutputValue(outputValue).ToBlock());
					}

					var parsedValue = FickleExpression.Variable(underlyingType, "parsedValue");
					var success = Expression.Variable(typeof(bool), "success");

					var parameters = new[] { new FickleParameterInfo(typeof(string), "value"), new FickleParameterInfo(underlyingType, "outValue", true) };
					var methodInfo = new FickleMethodInfo(null, typeof(bool), TypeSystem.GetPrimitiveName(underlyingType, true) + "TryParse", parameters, true);

					if (valueType.IsNullable())
					{
						outputValue = Expression.Convert(Expression.Condition(success, Expression.Convert(Expression.Convert(parsedValue, valueType), FickleType.Define("id")), Expression.Convert(FickleExpression.StaticCall(FickleType.Define("NSNull"), valueType, "null", null), FickleType.Define("id"))), valueType);
					}
					else
					{
						outputValue = Expression.Convert(parsedValue, valueType);
					}

					var secondIf = Expression.IfThenElse
					(
						Expression.TypeIs(value, FickleType.Define("NSString")),
						FickleExpression.Block
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
			else if (valueType is FickleType && ((FickleType)valueType).ServiceClass != null)
			{
				outputValue = FickleExpression.New(valueType, "initWithPropertyDictionary", FickleExpression.Convert(value, "NSDictionary"));

				return Expression.IfThen(Expression.TypeIs(value, FickleType.Define("NSDictionary")), processOutputValue(outputValue).ToBlock());
			}
			else if (valueType is FickleType && ((FickleType)valueType).ServiceEnum != null)
			{
				return Expression.IfThen(Expression.TypeIs(value, FickleType.Define("NSNumber")), processOutputValue(value).ToBlock());
			}
			else if (valueType is FickleListType)
			{
				var listType = valueType as FickleListType;

				var arrayVar = FickleExpression.Variable("NSMutableArray", "array");
				var variables = new[] { arrayVar };
				var arrayItem = FickleExpression.Parameter(FickleType.Define("id"), "arrayItem");

				var forEachBody = GetDeserializeExpressionProcessValueDeserializer(listType.ListElementType, arrayItem, c => FickleExpression.Call(arrayVar, "addObject", Expression.Convert(c, typeof(object))).ToStatement());
				
				var ifThen = Expression.IfThen
				(
					Expression.TypeIs(value, FickleType.Define("NSArray")),
					FickleExpression.Block
					(
						variables,
						Expression.Assign(arrayVar, FickleExpression.New("NSMutableArray", "initWithCapacity", FickleExpression.Call(Expression.Convert(value, FickleType.Define("NSArray")), typeof(int), "count", null))).ToStatement(),
						FickleExpression.ForEach(arrayItem, value, FickleExpression.Block(forEachBody)),
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
			var dictionaryType = new FickleType("NSDictionary");
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new FickleMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new FickleParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));
			var propertyExpression = Expression.Property(Expression.Parameter(this.type, "self"), new FicklePropertyInfo(this.type, property.PropertyType, property.PropertyName));
			
			var expressions = new List<Expression>
			{
				FickleExpression.Comment(property.PropertyName),
				Expression.Assign(currentValueFromDictionary, objectForKeyCall).ToStatement(),
				GetDeserializeExpressionProcessValueDeserializer(property.PropertyType, currentValueFromDictionary, c => Expression.Assign(propertyExpression, c).ToStatement())
			};

			this.propertyGetterExpressions.Add(expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
