using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows.Markup;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class EnumHeaderExpressionBinder
		: ServiceExpressionVisitor
	{
		private TypeDefinitionExpression currentTypeDefinition;

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new EnumHeaderExpressionBinder();

			return binder.Visit(expression);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Expression.Parameter(node.Type, currentTypeDefinition.Name.Capitalize() +  node.Name.Capitalize());
		}

		protected virtual Expression CreateToStringMethod()
		{
			var value = Expression.Parameter(currentTypeDefinition.Type, "value");
			var methodName = currentTypeDefinition.Name.Capitalize() + "ToString";

			var parameters = new Expression[]
			{
				value
			};

			var defaultBody = Expression.Return(Expression.Label(), Expression.Constant(null, typeof(string))).ToStatement();

			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((DryType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Return(Expression.Label(), Expression.Constant(enumValue.Name)).ToStatement(), Expression.Constant((int)enumValue.Value, currentTypeDefinition.Type)));
			}

			var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

			var body = DryExpression.Block(switchStatement);

			return new MethodDefinitionExpression(methodName, parameters.ToReadOnlyCollection(), typeof(string), body, false, "__unused", null, true, true);
		}

		protected virtual Expression CreateTryParseMethod()
		{
			var value = Expression.Parameter(typeof(string), "value");
			var methodName = currentTypeDefinition.Name.Capitalize() + "TryParse";
			var result = Expression.Parameter(currentTypeDefinition.Type.MakeByRefType(), "result");

			var parameters = new Expression[]
			{
				value,
				result
			};

			var defaultBody = Expression.Return(Expression.Label(), Expression.Constant(false)).ToStatement();
			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((DryType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Assign(result, Expression.Convert(Expression.Constant((int)enumValue.Value), currentTypeDefinition.Type)).ToStatement(), Expression.Constant(enumValue.Name)));
			}

			var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

			var body = DryExpression.Block(switchStatement, Expression.Return(Expression.Label(), Expression.Constant(true)));

			return new MethodDefinitionExpression(methodName, parameters.ToReadOnlyCollection(), typeof(bool), body, false, "__unused", null, true, true);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			try
			{
				currentTypeDefinition = expression;

				var body = this.Visit(expression.Body);

				return DryExpression.GroupedWide
				(
					new TypeDefinitionExpression(expression.Type, null, body, false, null, null),
					this.CreateTryParseMethod(),
					this.CreateToStringMethod()
				);
			}
			finally
			{
				currentTypeDefinition = null;
			}
		}
	}
}
