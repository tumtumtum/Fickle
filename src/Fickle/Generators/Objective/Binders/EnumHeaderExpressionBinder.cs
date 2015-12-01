using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
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
			return Expression.Parameter(node.Type, currentTypeDefinition.Type.Name.Capitalize() +  node.Name.Capitalize());
		}

		protected virtual Expression CreateToStringMethod()
		{
			var value = Expression.Parameter(currentTypeDefinition.Type, "value");
			var methodName = currentTypeDefinition.Type.Name.Capitalize() + "ToString";

			var parameters = new Expression[]
			{
				value
			};

			var array = FickleExpression.Variable("NSMutableArray", "array");
			var temp = FickleExpression.Variable(currentTypeDefinition.Type, "temp");

			var expressions = new List<Expression>
			{
				Expression.Assign(array, Expression.New(array.Type))
			};

			foreach (var enumValue in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				var currentEnumValue = Expression.Constant((int)enumValue.Value);

                expressions.Add
				(
					Expression.IfThen
					(
						Expression.Equal(Expression.And(currentEnumValue, Expression.Convert(value, typeof(int))), Expression.Convert(currentEnumValue, typeof(int))),
						FickleExpression.StatementisedGroupedExpression
						(
							GroupedExpressionsExpressionStyle.Wide,
							FickleExpression.Call(array, typeof(void), "addObject", Expression.Constant(enumValue.Name)),
							Expression.Assign(temp, Expression.Convert(Expression.Or(Expression.Convert(temp, typeof(int)), currentEnumValue), currentTypeDefinition.Type))
						).ToBlock()
					)
				);
			}

			expressions.Add(Expression.IfThen(Expression.NotEqual(value, temp), FickleExpression.Return(FickleExpression.Call(Expression.Convert(value, typeof(object)), typeof(string), "stringValue", null)).ToStatementBlock()));
			expressions.Add(FickleExpression.Return(FickleExpression.Call(array, "componentsJoinedByString", Expression.Constant(","))));

			var defaultBody = FickleExpression.StatementisedGroupedExpression
			(
				GroupedExpressionsExpressionStyle.Wide,
				expressions.ToArray()
			);

			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Return(Expression.Label(), Expression.Constant(enumValue.Name)).ToStatement(), Expression.Constant((int)enumValue.Value, currentTypeDefinition.Type)));
			}

			var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

			var body = FickleExpression.Block(new [] { array, temp }, switchStatement);

			return new MethodDefinitionExpression(methodName, parameters.ToReadOnlyCollection(), AccessModifiers.Static | AccessModifiers.ClasseslessFunction, typeof(string), body, false, "__unused", null);
		}

		protected virtual Expression CreateTryParseMethod()
		{
			var value = Expression.Parameter(typeof(string), "value");
			var methodName = currentTypeDefinition.Type.Name.Capitalize() + "TryParse";
			var result = Expression.Parameter(currentTypeDefinition.Type.MakeByRefType(), "result");
			var retval = Expression.Variable(currentTypeDefinition.Type, "retval");

			var parameters = new Expression[]
			{
				value,
				result
			};

			var parts = Expression.Variable(FickleType.Define("NSArray"), "parts");
			var splitCall = FickleExpression.Call(value, FickleType.Define("NSArray"), "componentsSeparatedByString", new { value = Expression.Constant(",") });
			var part = Expression.Variable(typeof(string), "part");
			var flagCases = new List<SwitchCase>();
			var number = Expression.Variable(FickleType.Define("NSNumber"), "number");
			
			foreach (var enumValue in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				flagCases.Add(Expression.SwitchCase(Expression.Assign(retval, Expression.Convert(Expression.Or(Expression.Convert(retval, typeof(int)), Expression.Constant((int)enumValue.Value)), currentTypeDefinition.Type)).ToStatement(), Expression.Constant(enumValue.Name)));
			}

			var foreachBody = FickleExpression.StatementisedGroupedExpression
			(
				Expression.Switch(part, FickleExpression.Return(Expression.Constant(false)).ToStatement(), flagCases.ToArray())
			).ToBlock();

			var defaultBody = FickleExpression.StatementisedGroupedExpression
			(
				GroupedExpressionsExpressionStyle.Wide,
				Expression.Assign(number, FickleExpression.Call(Expression.New(FickleType.Define("NSNumberFormatter")), number.Type, "numberFromString", value)),
				Expression.IfThen
				(
					Expression.NotEqual(number, Expression.Constant(null, number.Type)), 
					FickleExpression.StatementisedGroupedExpression
					(
						GroupedExpressionsExpressionStyle.Wide,
						Expression.Assign(result, Expression.Convert(FickleExpression.Call(number, typeof(int), "intValue", null), currentTypeDefinition.Type)),
						Expression.Return(Expression.Label(), Expression.Constant(true))
					).ToBlock()
				),
				Expression.Assign(parts, splitCall),
				Expression.Assign(retval, Expression.Convert(Expression.Constant(0), currentTypeDefinition.Type)),
                FickleExpression.ForEach(part, parts, foreachBody),
				Expression.Assign(result, retval),
				Expression.Return(Expression.Label(), Expression.Constant(true))
			);

			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Assign(result, Expression.Convert(Expression.Constant((int)enumValue.Value), currentTypeDefinition.Type)).ToStatement(), Expression.Constant(enumValue.Name)));
			}

			var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

			var body = FickleExpression.Block(new[] { parts, number, retval }, switchStatement, Expression.Return(Expression.Label(), Expression.Constant(true)));

			return new MethodDefinitionExpression(methodName, parameters.ToReadOnlyCollection(), AccessModifiers.Static | AccessModifiers.ClasseslessFunction, typeof(bool), body, false, "__unused", null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			try
			{
				currentTypeDefinition = expression;

				var body = this.Visit(expression.Body);

				var include = FickleExpression.Include("Foundation/Foundation.h");

				var comment = new CommentExpression("This file is AUTO GENERATED");
				var header = new Expression[] { comment, include  }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

				return FickleExpression.GroupedWide
				(
					new TypeDefinitionExpression(expression.Type, header, body, false, null, null),
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
