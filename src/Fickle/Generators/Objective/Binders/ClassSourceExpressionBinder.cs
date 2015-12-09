//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
{
	public class ClassSourceExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		
		private ClassSourceExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new ClassSourceExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		private Expression CreateScalarPropertiesAsFormEncodedStringMethod(TypeDefinitionExpression expression)
		{
			var self = Expression.Parameter(expression.Type, "self");
			var properties = ExpressionGatherer.Gather(expression, ServiceExpressionType.PropertyDefinition).Where(c => !(c.Type is FickleListType)).ToList();
			var parameters = properties.OfType<PropertyDefinitionExpression>().ToDictionary(c => c.PropertyName, c => Expression.Property(self, c.PropertyName));
			var path = string.Join("", properties.OfType<PropertyDefinitionExpression>().Select(c => c.PropertyName + "={" + c.PropertyName + "}"));

			var formatInfo = ObjectiveStringFormatInfo.GetObjectiveStringFormatInfo
			(
				path,
				c => parameters[c],
				(s, t) => t == typeof(string) ? s : s + "&",
				c => FickleExpression.Call(c, typeof(string), "stringByAppendingString", Expression.Constant("&"))
			);

			var parameterInfos = new List<ParameterInfo>
			{
				new FickleParameterInfo(typeof(string), "format")
			};

			parameterInfos.AddRange(formatInfo.ParameterExpressions.Select(c => new ObjectiveParameterInfo(c.Type, c.Name, true)));

			var args = new List<Expression>
			{
				Expression.Constant(formatInfo.Format)
			};

			args.AddRange(formatInfo.ValueExpressions);

			var methodInfo = new FickleMethodInfo(typeof(string), typeof(string), "stringWithFormat", parameterInfos.ToArray(), true);
			var methodBody = Expression.Block(FickleExpression.Return(Expression.Call(null, methodInfo, args)).ToStatement());

			return new MethodDefinitionExpression("scalarPropertiesAsFormEncodedString", new List<Expression>().ToReadOnlyCollection(), typeof(string), methodBody, false, null);
		}
		
		private Expression CreateAllPropertiesAsDictionaryMethod(TypeDefinitionExpression expression)
		{
			var dictionaryType = new FickleType("NSMutableDictionary");
			var retvalExpression = Expression.Parameter(dictionaryType, "retval");
			
			IEnumerable<ParameterExpression> variables = new[]
			{
				retvalExpression
			};

			var newDictionaryExpression = Expression.Assign(retvalExpression, FickleExpression.New("NSMutableDictionary", "initWithCapacity", ExpressionTypeCounter.Count(expression, (ExpressionType)ServiceExpressionType.PropertyDefinition) * 2)).ToStatement();
			var makeDictionaryExpression = PropertiesToDictionaryExpressionBinder.Build(expression, this.codeGenerationContext);
			var returnDictionaryExpression = Expression.Return(Expression.Label(), Expression.Parameter(dictionaryType, "retval")).ToStatement();

			var methodBody = Expression.Block(variables, GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, newDictionaryExpression, makeDictionaryExpression, returnDictionaryExpression));

			return new MethodDefinitionExpression("allPropertiesAsDictionary", new List<Expression>().ToReadOnlyCollection(), dictionaryType, methodBody, false, null);
		}

		private MethodDefinitionExpression CreateInitMethod(TypeDefinitionExpression expression)
		{
			var type = expression.Type;
			Expression superInitExpression;

			var parameters = new List<Expression>
			{
				Expression.Parameter(new FickleType("NSDictionary"), "properties")
			};

			var methodBodyExpressions = new List<Expression>();

			if (type.BaseType.IsServiceType())
			{
				superInitExpression = Expression.Call(Expression.Parameter(type.BaseType, "super"), new FickleMethodInfo(type.BaseType, type, "initWithPropertyDictionary", new [] { new FickleParameterInfo(parameters[0].Type, "dictionary")}), parameters[0]);
			}
			else
			{
				superInitExpression = Expression.Call(Expression.Parameter(type.BaseType, "super"), new FickleMethodInfo(type.BaseType, type, "init", new ParameterInfo[0]));
			}

			var assignExpression = Expression.Assign(Expression.Parameter(type, "self"), superInitExpression);
			var compareToNullExpression = Expression.ReferenceEqual(assignExpression, Expression.Constant(null, type));
			int count;

			methodBodyExpressions.Add(Expression.IfThen(compareToNullExpression, Expression.Block(Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement())));
			methodBodyExpressions.Add(PropertiesFromDictionaryExpressonBinder.Bind(expression, out count));
			methodBodyExpressions.Add(Expression.Return(Expression.Label(), Expression.Parameter(type, "self")).ToStatement());

			IEnumerable<ParameterExpression> variables;
			
			if (count > 0)
			{
				variables = new[] { Expression.Parameter(FickleType.Define("id"), "currentValueFromDictionary") };
			}
			else
			{
				variables = new ParameterExpression[0];
			}

			var methodBody = Expression.Block(variables, (Expression)methodBodyExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return new MethodDefinitionExpression("initWithPropertyDictionary", parameters.ToReadOnlyCollection(), typeof(object), methodBody, false, null);
		}

		private Expression CreateCopyWithZoneMethod(TypeDefinitionExpression expression)
		{
			var currentType = (FickleType)expression.Type; 
			var zone = FickleExpression.Parameter("NSZone", "zone");
			var self = Expression.Parameter(currentType, "self"); 
			var theCopy = Expression.Variable(expression.Type, "theCopy");

			Expression newExpression;

			if (expression.Type.BaseType == typeof(object))
			{
				newExpression = FickleExpression.Call(FickleExpression.Call(FickleExpression.Call(self, "Class", "class", null), "allocWithZone", zone), expression.Type, "init", null);
			}
			else
			{
				var super = FickleExpression.Variable(expression.Type.BaseType.Name, "super");

				newExpression = FickleExpression.Call(super, FickleType.Define("id"), "copyWithZone", zone);
			}

			var initTheCopy = Expression.Assign(theCopy, newExpression).ToStatement();
			var returnStatement = Expression.Return(Expression.Label(), theCopy).ToStatement();
			var copyStatements = PropertiesToCopyExpressionBinder.Bind(codeGenerationContext, expression, zone, theCopy);

			Expression methodBody = Expression.Block
			(
				new[] { theCopy },
				initTheCopy,
				FickleExpression.GroupedWide
				(
					copyStatements,
					returnStatement
				)
			);

			return new MethodDefinitionExpression("copyWithZone", new Expression[] { zone }.ToReadOnlyCollection(), typeof(object), methodBody, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>
			{
				FickleExpression.Include(expression.Type.Name + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			foreach (var referencedType in referencedTypes.Where(ObjectiveBinderHelpers.TypeIsServiceClass))
			{
				includeExpressions.Add(FickleExpression.Include(referencedType.Name + ".h"));
			}

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var methods = new List<Expression>
			{
				this.CreateInitMethod(expression),
				this.CreateAllPropertiesAsDictionaryMethod(expression),
				this.CreateScalarPropertiesAsFormEncodedStringMethod(expression),
				this.CreateCopyWithZoneMethod(expression),
			};

			var body = methods.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false, expression.Attributes, expression.InterfaceTypes);
		}
	}
}
