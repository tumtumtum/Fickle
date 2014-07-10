using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dryice.Expressions;
using Dryice.Model;

namespace Dryice.Generators
{
	public class PropertyTypeChanger
		: ServiceExpressionVisitor
	{
		private readonly string propertyName;
		private readonly Type newPropertyType;
		
		private PropertyTypeChanger(string propertyName, Type newPropertyType)
		{
			this.propertyName = propertyName;
			this.newPropertyType = newPropertyType;
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			if (string.Equals(property.PropertyName, propertyName, StringComparison.InvariantCultureIgnoreCase))
			{
				return new PropertyDefinitionExpression(property.PropertyName, this.newPropertyType);
			}

			return base.VisitPropertyDefinitionExpression(property);
		}

		public static Expression Change(Expression expression, string propertyName, Type newType)
		{
			return new PropertyTypeChanger(propertyName, newType).Visit(expression);
		}
	}
}
