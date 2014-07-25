//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dryice.Expressions;
using Dryice.Generators.Objective;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java.Binders
{
	public class ClassExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		
		private ClassExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new ClassExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		private Expression CreateAllPropertiesAsDictionaryMethod(TypeDefinitionExpression expression)
		{
			var dictionaryType = new DryType("NSMutableDictionary");
			var retvalExpression = Expression.Parameter(dictionaryType, "retval");
			
			IEnumerable<ParameterExpression> variables = new ParameterExpression[]
			{
				retvalExpression
			};

			var newDictionaryExpression = Expression.Assign(retvalExpression, DryExpression.New("NSMutableDictionary", "initWithCapacity", ExpressionTypeCounter.Count(expression, (ExpressionType)ServiceExpressionType.PropertyDefinition))).ToStatement();
			var makeDictionaryExpression = PropertiesToDictionaryExpressionBinder.Build(expression);
			var returnDictionaryExpression = Expression.Return(Expression.Label(), Expression.Parameter(dictionaryType, "retval")).ToStatement();

			var methodBody = Expression.Block(variables, (Expression)GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, newDictionaryExpression, makeDictionaryExpression, returnDictionaryExpression));

			return new MethodDefinitionExpression("allPropertiesAsDictionary", new ReadOnlyCollection<Expression>(new List<Expression>()), dictionaryType, methodBody, false, null);
		}

		private MethodDefinitionExpression CreateInitMethod(TypeDefinitionExpression expression)
		{
			var type = expression.Type;

			var parameters = new List<Expression>
			{
				Expression.Parameter(new DryType("NSDictionary"), "properties")
			};

			var methodBodyExpressions = new List<Expression>();
			var superInitExpression = Expression.Call(Expression.Parameter(type.BaseType, "super"), new DryMethodInfo(type.BaseType, type, "init", new ParameterInfo[0]));

			var assignExpression = Expression.Assign(Expression.Parameter(type, "self"), superInitExpression);
			var compareToNullExpression = Expression.ReferenceEqual(assignExpression, Expression.Constant(null, type));

			methodBodyExpressions.Add(Expression.IfThen(compareToNullExpression, Expression.Block(Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement())));
			methodBodyExpressions.Add(PropertiesFromDictionaryExpressonBinder.Bind(expression));
			methodBodyExpressions.Add(Expression.Return(Expression.Label(), Expression.Parameter(type, "self")).ToStatement());

			IEnumerable<ParameterExpression> variables = new ParameterExpression[]
			{
				Expression.Parameter(DryType.Define("id"), "currentValueFromDictionary")
			};

			var methodBody = Expression.Block(variables, (Expression)methodBodyExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide));

			return new MethodDefinitionExpression("initWithPropertyDictionary", new ReadOnlyCollection<Expression>(parameters), typeof(object), methodBody, false, null);
		}

		private Expression CreateCopyWithZoneMethod(TypeDefinitionExpression expression)
		{
			var currentType = (DryType)expression.Type; 
			var zone = DryExpression.Parameter("NSZone", "zone");
			var self = Expression.Parameter(currentType, "self"); 
			var theCopy = Expression.Variable(expression.Type, "theCopy");

			var newExpression = DryExpression.Call(DryExpression.Call(DryExpression.Call(self, "Class", "class", null), "allocWithZone", zone), expression.Type, "init", null);

			var initTheCopy = Expression.Assign(theCopy, newExpression).ToStatement();
			var returnStatement = Expression.Return(Expression.Label(), theCopy).ToStatement();
			var copyStatements = PropertiesToCopyExpressionBinder.Bind(codeGenerationContext, expression, zone, theCopy);

			Expression methodBody = Expression.Block
			(
				new[] { theCopy },
				initTheCopy,
				DryExpression.GroupedWide
				(
					copyStatements,
					returnStatement
				)
			);

			return new MethodDefinitionExpression("copyWithZone", new Expression[] { zone }.ToReadOnlyCollection(), typeof(object), methodBody, false, null);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.Uncapitalize();

			var propertyDefinition = new PropertyDefinitionExpression(name, property.PropertyType, true);

			if (name.StartsWith("new"))
			{
				var attributedPropertyGetter = new MethodDefinitionExpression(name, null, property.PropertyType, null, true, "(objc_method_family(none))", null);

				return new Expression[] { propertyDefinition, attributedPropertyGetter }.ToStatementisedGroupedExpression();
			}
			else
			{
				return propertyDefinition;
			}
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var includeExpressions = referencedTypes
				.Where(ObjectiveBinderHelpers.TypeIsServiceClass)
				.Where(c => c != expression.Type.BaseType)
				.Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			foreach (var referencedType in referencedTypes.Where(JavaBinderHelpers.TypeIsServiceClass))
			{
				includeExpressions.Add(DryExpression.Include(referencedType.Name));
			}

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.UUID"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.Date"));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(DryExpression.Include(expression.Type.BaseType.Name));
			}

			includeExpressions.Add(DryExpression.Include("java.util.Dictionary"));

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var methods = new List<Expression>
			{
				this.Visit(expression.Body),
				this.CreateInitMethod(expression),
				this.CreateAllPropertiesAsDictionaryMethod(expression),
				this.CreateCopyWithZoneMethod(expression),
			};

			var body = methods.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var interfaceTypes = new List<Type>
			{
				DryType.Define("NSCopying"),
				DryType.Define("PKDictionarySerializable")
			};

			var interfaces = interfaceTypes.Append(expression.InterfaceTypes);

			return new TypeDefinitionExpression(expression.Type, header, body, false, null, interfaceTypes.ToReadOnlyCollection());
		}
	}
}
