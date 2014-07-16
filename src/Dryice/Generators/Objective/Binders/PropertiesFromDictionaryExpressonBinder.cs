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

		private void ProcessPropertyDeserializer(Type propertyType, string propertyName, Expression value, out Type typeToCompare, out Expression[] processingStatements, out Expression outputValue, out ParameterExpression[] variables)
		{
			variables = null;

			if (TypeSystem.IsPrimitiveType(propertyType))
			{
				var underlyingType = DryNullable.GetUnderlyingType(propertyType) ?? propertyType;

				if (underlyingType == typeof(byte)
					|| underlyingType == typeof(char) || underlyingType == typeof(short)
					|| underlyingType == typeof(int) || underlyingType == typeof(long))
				{
					typeToCompare = new DryType("NSNumber");
				}
				else
				{
					typeToCompare = typeof(string);
				}

				processingStatements = null;
				outputValue = Expression.Convert(value, propertyType);
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
				Type typeToCompareInner;
				Expression outputValueInner = null;
				Expression[] processingStatementsInner;
				ParameterExpression[] variablesInner;
				var listType = propertyType as DryListType;

				typeToCompare = new DryType("NSArray");

				var arrayVar = DryExpression.Variable("NSMutableArray", propertyName.Uncapitalize() + "Array");
				variables = new[] { arrayVar };

				var arrayItem = DryExpression.Parameter(typeof(object), "arrayItem");
				var constructedArrayItem = DryExpression.Parameter(listType.ListElementType, "constructedArrayItem");

				this.ProcessPropertyDeserializer(listType.ListElementType, listType.ListElementType.Name.Uncapitalize(), arrayItem, out typeToCompareInner, out processingStatementsInner, out outputValueInner, out variablesInner);

				var statements = new List<Expression>();
				if (processingStatementsInner != null)
				{
					statements.AddRange(processingStatementsInner);
				}

				statements.Add(Expression.Assign(constructedArrayItem, outputValueInner).ToStatement());
				statements.Add(DryExpression.Call(arrayVar, "addObject", constructedArrayItem).ToStatement());

				var forEachBodyStatements = new Expression[]
				{
					Expression.IfThen(Expression.TypeEqual(arrayItem, typeToCompareInner), 
					Expression.Block(new [] { constructedArrayItem }, statements.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide)))
				};

				processingStatements = new Expression[]
				{
					Expression.Assign(arrayVar, DryExpression.New("NSMutableArray", "initWithCapacity", DryExpression.Call(value, typeof(int), "count", null))).ToStatement(),
					DryExpression.ForEach(arrayItem, value, Expression.Block(forEachBodyStatements.ToStatementisedGroupedExpression()))
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
			var comment = new CommentExpression(property.PropertyName);
			var expressions = new List<Expression>();
			var dictionaryType = new DryType("NSDictionary"); 
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new DryMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new DryParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));

			var propertyExpression = Expression.Property(Expression.Parameter(this.type, "self"), new DryPropertyInfo(this.type, property.PropertyType, property.PropertyName.Uncapitalize()));

			Type typeToCompare;
			Expression outputValue = null;
			Expression[] processingStatements;
			ParameterExpression[] variables;

			this.ProcessPropertyDeserializer(property.PropertyType, property.PropertyName, currentValueFromDictionary, out typeToCompare, out processingStatements, out outputValue, out variables);

			expressions.Add(comment);
			expressions.Add(Expression.Assign(currentValueFromDictionary, objectForKeyCall).ToStatement());

			var statements = new List<Expression>();
			if (processingStatements != null)
			{
				statements.AddRange(processingStatements);
			}
			
			statements.Add(Expression.Assign(propertyExpression,outputValue).ToStatement());

			expressions.Add(Expression.IfThen(Expression.TypeIs(currentValueFromDictionary, typeToCompare), Expression.Block(variables, new GroupedExpressionsExpression(statements, GroupedExpressionsExpressionStyle.Wide))));

			this.propertyGetterExpressions.Add(expressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
