using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dryice.Expressions;
using Dryice.Model;

namespace Dryice.Generators.Objective
{
	public class PropertiesToCopyExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly Type type; 
		private readonly ServiceModel serviceModel;
		private readonly Expression zone;
		private readonly Expression theCopy;
		private readonly List<Expression> statements = new List<Expression>();
 
		protected PropertiesToCopyExpressionBinder(ServiceModel serviceModel, Type type, Expression zone, Expression theCopy)
		{
			this.serviceModel = serviceModel;
			this.type = type;
			this.zone = zone;
			this.theCopy = theCopy;
		}

		public static Expression Bind(ServiceModel serviceModel, TypeDefinitionExpression expression, ParameterExpression zone, ParameterExpression theCopy)
		{
			var builder = new PropertiesToCopyExpressionBinder(serviceModel, expression.Type, zone, theCopy);

			builder.Visit(expression);

			return builder.statements.ToGroupedExpression();
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Parameter(theCopy.Type, "self");

			var propertyOnTheCopy = Expression.Property(theCopy, property.PropertyName);
			Expression p2 = Expression.Property(self, property.PropertyName);

			var propertyType = propertyOnTheCopy.Type;

			if (propertyType.GetDryiceListElementType() != null)
			{
				var constructorInfo = ObjectiveLanguage.MakeConstructorInfo(propertyType, "initWithArray", theCopy.Type, "oldArray", typeof(bool), "copyItems");
				
				p2 = Expression.New(constructorInfo, p2, Expression.Constant(true));
			}
			else if (propertyType is DryiceType && !propertyType.IsValueType)
			{
				p2 = Expression.Call(p2, ((DryiceType)propertyType).GetMethod("copyWithZone", typeof(object), ObjectiveLanguage.NSZoneType), zone);
				p2 = Expression.Convert(p2, propertyType);
			}

			var assignExpression = Expression.Assign(propertyOnTheCopy, p2);

			statements.Add(new StatementExpression(assignExpression));

			return property;
		}
	}
}
