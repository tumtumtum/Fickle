//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Platform;
using Dryice.Expressions;

namespace Dryice.Generators.Objective
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

			return builder.propertyGetterExpressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);
		}

		private void ProcessPropertyDeserializer(Type propertyType, string propertyName, Expression value, out Type typeToCompare, out Expression[] processingStatements, out Expression outputValue, out ParameterExpression[] variables)
		{
			variables = null;

			if (TypeSystem.IsPrimitiveType(propertyType))
			{
				var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

				if (underlyingType == typeof(byte)
					|| underlyingType == typeof(char) || underlyingType == typeof(short)
					|| underlyingType == typeof(int) || underlyingType == typeof(long))
				{
					typeToCompare = new DryiceType("NSNumber");
				}
				else
				{
					typeToCompare = typeof(string);
				}

				processingStatements = null;
				outputValue = Expression.Convert(value, propertyType);
			}
			else if (propertyType is DryiceType && ((DryiceType)propertyType).ServiceClass != null)
			{
				typeToCompare = ObjectiveLanguage.NSDictionary;

				processingStatements = null;
				outputValue = Expression.New(((DryiceType)propertyType).GetConstructor("initWithPropertyDictionary", ObjectiveLanguage.NSDictionary), Expression.Convert(value, ObjectiveLanguage.NSDictionary));
			}
			else if (propertyType is DryiceType && ((DryiceType)propertyType).ServiceEnum != null)
			{
				typeToCompare = new DryiceType("NSNumber");

				processingStatements = null;
				outputValue = Expression.Convert(value, propertyType);
			}
			else if (propertyType is DryiceListType)
			{
				var listType = propertyType as DryiceListType;

				typeToCompare = new DryiceType("NSArray");

				var arrayVar = Expression.Variable(ObjectiveLanguage.NSMutableArray, propertyName.Uncapitalize() + "Array");
				variables = new[] { arrayVar };

				var constructorInfo = ObjectiveLanguage.MakeConstructorInfo(ObjectiveLanguage.NSMutableArray, "initWithCapacity", typeof(int), "capacity");

				var arrayItem = Expression.Parameter(typeof(object), "arrayItem");

				var constructedArrayItem = Expression.Parameter(listType.ListElementType, "constructedArrayItem");

				Type typeToCompareInner;
				Expression outputValueInner = null;
				Expression[] processingStatementsInner;
				ParameterExpression[] variablesInner;

				this.ProcessPropertyDeserializer(listType.ListElementType, listType.ListElementType.Name.Uncapitalize(), arrayItem, out typeToCompareInner, out processingStatementsInner, out outputValueInner, out variablesInner);

				var statements = new List<Expression>();
				if (processingStatementsInner != null)
				{
					statements.AddRange(processingStatementsInner);
				}
				statements.Add(new StatementExpression(Expression.Assign(constructedArrayItem, outputValueInner)));
				statements.Add(new StatementExpression(ObjectiveLanguage.MakeCall(arrayVar, typeof(void), "addObject", constructedArrayItem)));

				var forEachBodyStatements = new Expression[]
				{
					Expression.IfThen(Expression.TypeEqual(arrayItem, typeToCompareInner), 
					Expression.Block(new [] { constructedArrayItem }, new GroupedExpressionsExpression(statements, GroupedExpressionsExpressionStyle.Wide)))
				};

				processingStatements = new Expression[]
				{
					new StatementExpression(Expression.Assign(arrayVar, Expression.New(constructorInfo, Expression.Property(value, new DryicePropertyInfo(typeof(object), typeof(int), "count"))))),
					new ForEachExpression(arrayItem, value, Expression.Block(new GroupedExpressionsExpression(forEachBodyStatements)))
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
			var dictionaryType = new DryiceType("NSDictionary"); 
			var currentValueFromDictionary = Expression.Parameter(typeof(object), "currentValueFromDictionary");
			var objectForKeyCall = Expression.Call(Expression.Parameter(dictionaryType, "properties"), new DryiceMethodInfo(dictionaryType, typeof(object), "objectForKey", new ParameterInfo[] { new DryiceParameterInfo(typeof(string), "key") }), Expression.Constant(property.PropertyName));

			var propertyExpression = Expression.Property(Expression.Parameter(type, "self"), new DryicePropertyInfo(type, property.PropertyType, property.PropertyName.Uncapitalize()));

			Type typeToCompare;
			Expression outputValue = null;
			Expression[] processingStatements;
			ParameterExpression[] variables;

			this.ProcessPropertyDeserializer(property.PropertyType, property.PropertyName, currentValueFromDictionary, out typeToCompare, out processingStatements, out outputValue, out variables);

			expressions.Add(comment);
			expressions.Add(new StatementExpression(Expression.Assign(currentValueFromDictionary, objectForKeyCall)));

			var statements = new List<Expression>();
			if (processingStatements != null)
			{
				statements.AddRange(processingStatements);
			}
			statements.Add(new StatementExpression(Expression.Assign(propertyExpression, outputValue)));
			
			expressions.Add(Expression.IfThen(Expression.TypeIs(currentValueFromDictionary, typeToCompare), Expression.Block(variables, new GroupedExpressionsExpression(statements, GroupedExpressionsExpressionStyle.Wide))));

			this.propertyGetterExpressions.Add(expressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return property;
		}
	}
}
