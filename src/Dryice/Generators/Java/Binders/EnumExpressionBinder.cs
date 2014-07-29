using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Markup;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Java.Binders
{
	public class EnumExpressionBinder
		: ServiceExpressionVisitor
	{
		private TypeDefinitionExpression currentTypeDefinition;

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new EnumExpressionBinder();

			return binder.Visit(expression);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Expression.Parameter(node.Type, currentTypeDefinition.Type.Name.Capitalize() +  node.Name.Capitalize());
		}

		protected virtual Expression CreateToStringMethod()
		{
			var value = Expression.Parameter(currentTypeDefinition.Type, "value");

			var defaultBody = Expression.Return(Expression.Label(), Expression.Constant(null, typeof(string))).ToStatement();

			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((DryType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Return(Expression.Label(), Expression.Constant(enumValue.Name)).ToStatement(), Expression.Constant(enumValue.Name, currentTypeDefinition.Type)));
			}

			var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

			var body = DryExpression.Block(switchStatement);

			return new MethodDefinitionExpression("toString", new List<Expression>(), AccessModifiers.Public, typeof(string), body, false, null, null);
		}

		protected virtual Expression CreateTryParseMethod()
		{
			var value = Expression.Parameter(typeof(string), "value");

			var cases = new List<SwitchCase>();

			foreach (var enumValue in ((DryType)currentTypeDefinition.Type).ServiceEnum.Values)
			{
				cases.Add(Expression.SwitchCase(Expression.Return(Expression.Label(), Expression.Constant(enumValue.Name, currentTypeDefinition.Type)).ToStatement(), Expression.Constant(enumValue.Name)));
			}

			var switchStatement = Expression.Switch(value, null, cases.ToArray());

			var body = DryExpression.Block(switchStatement, Expression.Return(Expression.Label(), Expression.Constant(null)));

			return new MethodDefinitionExpression("tryParse", new List<Expression>(), AccessModifiers.Public, currentTypeDefinition.Type, body, false, null, null);
		}

		protected virtual Expression CreateConstructor(FieldDefinitionExpression value)
		{
			var valParam = Expression.Parameter(typeof(int), "value");

			var parameters = new Expression[]
			{
				valParam
			};

			var valueMember = Expression.Variable(value.PropertyType, "this." + value.PropertyName);

			var body = DryExpression.Block(Expression.Assign(valueMember, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinition.Type.Name, parameters.ToReadOnlyCollection(), null, body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			try
			{
				currentTypeDefinition = expression;

				var valueProperty = new FieldDefinitionExpression("value", typeof(int), AccessModifiers.Private | AccessModifiers.Constant);

				var includeExpressions = new List<Expression>() { DryExpression.Include("java.lang.reflect.Type") };
				var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
				referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

				foreach (var referencedType in referencedTypes.Where(JavaBinderHelpers.TypeIsServiceClass))
				{
					includeExpressions.Add(DryExpression.Include(referencedType.Name));
				}
				var includeStatements = includeExpressions.ToStatementisedGroupedExpression();

				var comment = new CommentExpression("This file is AUTO GENERATED");
				var header = new Expression[] { comment, includeStatements }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

				var bodyExpressions = new List<Expression>()
				{
					expression.Body,
					valueProperty,
					CreateConstructor(valueProperty),
					//CreateTryParseMethod(),
					CreateToStringMethod()
				};

				var body = new GroupedExpressionsExpression(bodyExpressions);

				return new TypeDefinitionExpression(expression.Type, header, body, false);
			}
			finally
			{
				currentTypeDefinition = null;
			}
		}
	}
}
