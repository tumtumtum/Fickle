//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Sublimate.Expressions;
using Sublimate.Model;

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

		private MethodDefinitionExpression CreateInitMethod(TypeDefinitionExpression expression)
		{
			var type = expression.Type;

			var parameters = new List<Expression>
			{	
				new ParameterDefinitionExpression("properties", new SublimateType("NSDictionary"), 0)
			};

			var methodBodyExpressions = new List<Expression>();
			var superInitExpression = Expression.Call(Expression.Parameter(type.BaseType, "super"), new SublimateMethodInfo(type.BaseType, type, "init", new ParameterInfo[0]));

			var assignExpression = Expression.Assign(Expression.Parameter(type, "self"), superInitExpression);
			var compareToNullExpression = Expression.ReferenceEqual(assignExpression, Expression.Constant(null, type));

			methodBodyExpressions.Add(Expression.IfThen(compareToNullExpression, Expression.Block(new StatementsExpression(Expression.Return(Expression.Label(), Expression.Constant(null))))));
			methodBodyExpressions.Add(PropertySetterExpressionsBuilder.Build(expression));
			methodBodyExpressions.Add(new StatementsExpression(Expression.Return(Expression.Label(), Expression.Parameter(type, "self"))));

			IEnumerable<ParameterExpression> variables = new ParameterExpression[]
			{
				Expression.Parameter(typeof(object), "currentValueFromDictionary")
			};
			
			var methodBody = Expression.Block(variables, (Expression)new GroupedExpressionsExpression(methodBodyExpressions));

			return new MethodDefinitionExpression("initWithPropertyDictionary", new ReadOnlyCollection<Expression>(parameters), typeof(object), methodBody, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>
			{
				new IncludeStatementExpression(expression.Name + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var commentGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new List<Expression> { comment }), true);
			var headerGroup = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(includeExpressions), true);
			
			var header = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(new[] { commentGroup, headerGroup }));

			var methods = new List<Expression>
			{
				this.CreateInitMethod(expression)
			};

			var body = new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(methods));

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, false, null);
		}
	}
}
