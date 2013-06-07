using System.Linq.Expressions;
using Sublimate.Model;

namespace Sublimate.Expressions
{
	public  class PropertyDefinitionExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.PropertyDefinition;
			}
		}

		public PropertyDefinitionExpression(string propertyName, ServiceType propertyType)
			: this(propertyName, propertyType, false)
		{	
		}

		public PropertyDefinitionExpression(string propertyName, ServiceType propertyType, bool isPredeclatation)
		{	
			this.PropertyType = propertyType;
			this.PropertyName = propertyName;
			this.IsPredeclatation = isPredeclatation;
		}

		public string PropertyName { get; private set; }
		public bool IsPredeclatation { get; private set; }
		public ServiceType PropertyType { get; private set; }
	}
}
