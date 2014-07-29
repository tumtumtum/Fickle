//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
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

		private void ProcessPropertyDeserializer(Type propertyType, string propertyName, Expression value, out Type typeToCompare, out Expression[] processingStatements, out Expression outputValue, out ParameterExpression[] variables, int parseLevel)
		{
			variables = null;

			if (TypeSystem.IsPrimitiveType(propertyType))
			{
				var underlyingType = propertyType.GetUnwrappedNullableType();

				processingStatements = null;

				if (underlyingType.IsNumericType() || underlyingType == typeof(bool))
				{
					typeToCompare = new DryType("NSNumber");

					if (underlyingType.IsEnum && propertyType.GetUnderlyingType() == null)
					{
						outputValue = Expression.Convert(DryExpression.Call(value, typeof(int), "intValue", null), propertyType);
					}
					else
					{
						outputValue = Expression.Convert(value, propertyType);
					}
				}
				else if (underlyingType.IsEnum)
				{
					if (parseLevel == 1)
					{
						typeToCompare = DryType.Define("NSNumber");

						if (propertyType.IsNullable())
						{
							outputValue = Expression.Convert(value, propertyType);
						}
						else
						{
							outputValue = Expression.Convert(DryExpression.Call(value, typeof(int), "intValue", null), propertyType);
						}
					}
					else
					{
						var parsedValue = DryExpression.Variable(underlyingType, "parsedValue");
						var success = DryExpression.Variable(typeof(bool), "success");

						variables = new ParameterExpression[]
						{
							parsedValue,
							success
						};
						
						var parameters = new[] { new DryParameterInfo(typeof(string), "value"), new DryParameterInfo(underlyingType, "outValue", true) };
						var methodInfo = new DryMethodInfo(null, typeof(bool), TypeSystem.GetPrimitiveName(underlyingType, true) + "TryParse", parameters, true);

						processingStatements = new Expression[]
						{
							Expression.Assign(parsedValue, Expression.Convert(Expression.Constant(0), parsedValue.Type)).ToStatement(),
							Expression.Assign(success, Expression.Call(null, methodInfo, Expression.Convert(value, typeof(string)), parsedValue)).ToStatement()
						};

						typeToCompare = DryType.Define("NSString");

						outputValue = parsedValue;

						if (propertyType.IsNullable())
						{
							outputValue = Expression.Convert(Expression.Condition(success, Expression.Convert(Expression.Convert(outputValue, propertyType), DryType.Define("id")), Expression.Convert(DryExpression.StaticCall(DryType.Define("NSNull"), propertyType, "null", null), DryType.Define("id"))), propertyType);
						}
						else
						{
							outputValue = Expression.Convert(outputValue, propertyType);
						}
					}
				}
				else
				{
					typeToCompare = typeof(string);

					outputValue = Expression.Convert(value, propertyType);
				}
			}
			else if (propertyType is DryType && ((DryType)propertyType).ServiceClass != null)
			{
				typeToCompare = new DryType("NSDictionary");

				processingStatements = null;
				outputValue = DryExpression.New(propertyType, "initWithPropertyDictionary", DryExpression.Convert(value, "NSDictionary"));
			}
			else if (propertyType is DryType && ((DryType)propertyType).ServiceEnum != null)
			{
				typeToCompare = new DryType("NSNumber");

				processingStatements = null;
				outputValue = Expression.Convert(value, propertyType);
			}
			else if (propertyType is DryListType)
			{
				processingStatements = null;

				Type typeToCompareInner;
				Expression outputValueInner = null;
				Expression[] processingStatementsInner;
				ParameterExpression[] variablesInner;
				var listType = propertyType as DryListType;

				typeToCompare = new DryType("NSArray");

				var arrayVar = DryExpression.Variable("NSMutableArray", propertyName.Uncapitalize() + "Array");
				variables = new[] { arrayVar };

				var arrayItem = DryExpression.Parameter(DryType.Define("id"), "arrayItem");

				Expression expressionForEachBody = null;

				for (var i = 1; i <= ((listType.ListElementType.GetUnwrappedNullableType().IsEnum) ? 2 : 1); i++)
				{
					var constructedArrayItem = DryExpression.Parameter(listType.ListElementType, "constructedArrayItem");

					this.ProcessPropertyDeserializer(listType.ListElementType, listType.ListElementType.Name.Uncapitalize(), arrayItem, out typeToCompareInner, out processingStatementsInner, out outputValueInner, out variablesInner, i);

					var statements = new List<Expression>();
					if (processingStatementsInner != null)
					{
						statements.AddRange(processingStatementsInner);
					}

					statements.Add(Expression.Assign(constructedArrayItem, outputValueInner).ToStatement());
					statements.Add(DryExpression.Call(arrayVar, "addObject", constructedArrayItem).ToStatement());

					variablesInner = variablesInner ?? new ParameterExpression[0];
					
					if (expressionForEachBody == null)
					{
						expressionForEachBody = Expression.IfThen(Expression.TypeEqual(arrayItem, typeToCompareInner), Expression.Block(variablesInner.Append(constructedArrayItem), statements.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide)));
					}
					else
					{
						expressionForEachBody = Expression.IfThenElse(Expression.TypeEqual(arrayItem, typeToCompareInner), Expression.Block(variablesInner.Append(constructedArrayItem), statements.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide)), expressionForEachBody);
					}
				}

				processingStatements = new Expression[]
				{
					Expression.Assign(arrayVar, DryExpression.New("NSMutableArray", "initWithCapacity", DryExpression.Call(value, typeof(int), "count", null))).ToStatement(),
					DryExpression.ForEach(arrayItem, value, DryExpression.Block(expressionForEachBody))
				};

				outputValue = Expression.Convert(arrayVar, propertyType);
			}
			else
			{
				throw new InvalidOperationException("Unsupported property type: " + propertyType);
			}
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var dictionaryType = new DryType("NSDictionary"); 
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new DryMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new DryParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));

			var propertyExpression = Expression.Property(Expression.Parameter(this.type, "self"), new DryPropertyInfo(this.type, property.PropertyType, property.PropertyName.Uncapitalize()));

			for (var i = 1; i <= (property.PropertyType.GetUnwrappedNullableType().IsEnum ? 2 : 1); i++)
			{
				Type typeToCompare;
				Expression outputValue = null;
				Expression[] processingStatements;
				ParameterExpression[] variables;
				var comment = new CommentExpression(property.PropertyName);
				var expressions = new List<Expression>();

				this.ProcessPropertyDeserializer(property.PropertyType, property.PropertyName, currentValueFromDictionary, out typeToCompare, out processingStatements, out outputValue, out variables, i);

				expressions.Add(comment);
				expressions.Add(Expression.Assign(currentValueFromDictionary, objectForKeyCall).ToStatement());

				var statements = new List<Expression>();

				if (processingStatements != null)
				{
					statements.AddRange(processingStatements);
				}

				statements.Add(Expression.Assign(propertyExpression, outputValue).ToStatement());

				expressions.Add(Expression.IfThen(Expression.TypeIs(currentValueFromDictionary, typeToCompare), Expression.Block(variables, GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, statements.ToArray()))));

				this.propertyGetterExpressions.Add(expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));
			}

			return property;
		}
	}
}
