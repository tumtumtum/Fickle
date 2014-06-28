using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Sublimate.Expressions;
using Sublimate.Model;

namespace Sublimate.Generators.Objective
{
	public class CopyPropertiesExpressionBuilder
		: ServiceExpressionVisitor
	{
		private readonly ServiceModel serviceModel;
		private readonly Type type;
		private readonly Expression zone;
		private readonly Expression theCopy;
		private readonly List<Expression> statements = new List<Expression>();
 
		protected CopyPropertiesExpressionBuilder(ServiceModel serviceModel, Type type, Expression zone, Expression theCopy)
		{
			this.serviceModel = serviceModel;
			this.type = type;
			this.zone = zone;
			this.theCopy = theCopy;
		}

		public static Expression Build(ServiceModel serviceModel, TypeDefinitionExpression expression, ParameterExpression zone, ParameterExpression theCopy)
		{
			var builder = new CopyPropertiesExpressionBuilder(serviceModel, expression.Type, zone, theCopy);

			builder.Visit(expression);

			return builder.statements.ToGroupedExpression();
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var self = Expression.Parameter(theCopy.Type, "self");

			var propertyOnTheCopy = Expression.Property(theCopy, property.PropertyName);
			Expression p2 = Expression.Property(self, property.PropertyName);

			var propertyType = propertyOnTheCopy.Type;

			if (propertyType.GetSublimateListElementType() != null)
			{
				var constructorInfo = ObjectiveLanguage.MakeConstructorInfo(propertyType, "initWithArray", theCopy.Type, "oldArray", typeof(bool), "copyItems");
				
				p2 = Expression.New(constructorInfo, p2, Expression.Constant(true));
			}
			else if (propertyType is SublimateType && !propertyType.IsValueType)
			{
				p2 = Expression.Call(p2, ((SublimateType)propertyType).GetMethod("copyWithZone", typeof(object), ObjectiveLanguage.NSZoneType), zone);
				p2 = Expression.Convert(p2, propertyType);
			}

			var assignExpression = Expression.Assign(propertyOnTheCopy, p2);

			statements.Add(new StatementExpression(assignExpression));

			return property;
		}
	}
}
