using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
{
	public class PropertiesToCopyExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		private readonly Type type; 
		private readonly Expression zone;
		private readonly Expression theCopy;
		private readonly List<Expression> statements = new List<Expression>();

		protected PropertiesToCopyExpressionBinder(CodeGenerationContext codeGenerationContext, Type type, Expression zone, Expression theCopy)
		{
			this.codeGenerationContext = codeGenerationContext;
			this.type = type;
			this.zone = zone;
			this.theCopy = theCopy;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression, ParameterExpression zone, ParameterExpression theCopy)
		{
			var builder = new PropertiesToCopyExpressionBinder(codeGenerationContext, expression.Type, zone, theCopy);

			builder.Visit(expression);

			return builder.statements.ToStatementisedGroupedExpression();
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Parameter(this.theCopy.Type, "self");

			var propertyOnTheCopy = Expression.Property(this.theCopy, property.PropertyName.Uncapitalize());
			var propertyOnSelf = (Expression)Expression.Property(self, property.PropertyName.Uncapitalize());

			var propertyType = propertyOnTheCopy.Type;

			if (propertyType.GetFickleListElementType() != null)
			{
				propertyOnSelf = FickleExpression.New(propertyType, "initWithArray", new
				{
					oldArray = propertyOnSelf,
					copyItems = true
				});
			}
			else if (propertyType is FickleType && !propertyType.IsValueType)
			{
				propertyOnSelf = Expression.Convert(FickleExpression.Call(propertyOnSelf, typeof(object), "copyWithZone", this.zone), propertyType);
			}
			else if (propertyType == typeof(string))
			{
				propertyOnSelf = FickleExpression.Call(propertyOnSelf, typeof(string), "copy", null);
			}

			var assignExpression = Expression.Assign(propertyOnTheCopy, propertyOnSelf);

			this.statements.Add(assignExpression.ToStatement());

			return property;
		}
	}
}
