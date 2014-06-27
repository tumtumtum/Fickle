//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Sublimate.Expressions;

namespace Sublimate.Generators.Objective
{
	public class ObjectiveClassExpressionBinder
		: ServiceExpressionVisitor
	{
		public static Expression Bind(Expression expression)
		{
			var binder = new ObjectiveClassExpressionBinder();

			return binder.Visit(expression);
		}

		private Expression CreateAllPropertiesAsDictionaryMethod(TypeDefinitionExpression expression)
		{
			var dictionaryType = new SublimateType("NSMutableDictionary");
			var retvalExpression = Expression.Parameter(dictionaryType, "retval");
			
			IEnumerable<ParameterExpression> variables = new ParameterExpression[]
			{
				retvalExpression
			};

			var newDictionaryExpression = new StatementsExpression(Expression.Assign(retvalExpression, Expression.New(dictionaryType.GetConstructor("dictionaryWithCapacity", typeof(int)), Expression.Constant(16))));
			var makeDictionaryExpression = MakeDictionaryFromPropertiesExpressonsBuilder.Build(expression);
			var returnDictionaryExpression = new StatementsExpression(Expression.Return(Expression.Label(), Expression.Parameter(dictionaryType, "retval")));

			var methodBody = Expression.Block(variables, (Expression)GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, newDictionaryExpression, makeDictionaryExpression, returnDictionaryExpression));

			return new MethodDefinitionExpression("allPropertiesAsDictionary", new ReadOnlyCollection<Expression>(new List<Expression>()), dictionaryType, methodBody, false, null);
		}

		private MethodDefinitionExpression CreateInitMethod(TypeDefinitionExpression expression)
		{
			var type = expression.Type;

			var parameters = new List<Expression>
			{	
				new ParameterDefinitionExpression("properties", new SublimateType("NSMutableDictionary"), 0)
			};

			var methodBodyExpressions = new List<Expression>();
			var superInitExpression = Expression.Call(Expression.Parameter(type.BaseType, "super"), new SublimateMethodInfo(type.BaseType, type, "init", new ParameterInfo[0]));

			var assignExpression = Expression.Assign(Expression.Parameter(type, "self"), superInitExpression);
			var compareToNullExpression = Expression.ReferenceEqual(assignExpression, Expression.Constant(null, type));

			methodBodyExpressions.Add(Expression.IfThen(compareToNullExpression, Expression.Block(new StatementsExpression(Expression.Return(Expression.Label(), Expression.Constant(null))))));
			methodBodyExpressions.Add(SetPropertiesFromDictionaryExpressonsBuilder.Build(expression));
			methodBodyExpressions.Add(new StatementsExpression(Expression.Return(Expression.Label(), Expression.Parameter(type, "self"))));

			IEnumerable<ParameterExpression> variables = new ParameterExpression[]
			{
				Expression.Parameter(typeof(object), "currentValueFromDictionary")
			};
			
			var methodBody = Expression.Block(variables, (Expression)new GroupedExpressionsExpression(methodBodyExpressions, GroupedExpressionsExpressionStyle.Wide));

			return new MethodDefinitionExpression("initWithPropertyDictionary", new ReadOnlyCollection<Expression>(parameters), typeof(object), methodBody, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>
			{
				new IncludeStatementExpression(expression.Name + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new List<Expression> { comment }), GroupedExpressionsExpressionStyle.Narrow);
			var headerGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(includeExpressions), GroupedExpressionsExpressionStyle.Narrow);
			
			var header = new GroupedExpressionsExpression(new Expression[] { commentGroup, headerGroup }, GroupedExpressionsExpressionStyle.Wide);

			var methods = new List<Expression>
			{
				this.CreateInitMethod(expression),
				this.CreateAllPropertiesAsDictionaryMethod(expression)
			};

			var body = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(methods), GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, false, null);
		}
	}
}
