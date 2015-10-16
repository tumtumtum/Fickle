using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public  class PropertyDefinitionExpression
		: BaseExpression
	{
		public override Type Type => typeof(void);
		public override ExpressionType NodeType => (ExpressionType)ServiceExpressionType.PropertyDefinition;
		public string PropertyName { get; private set; }
		public bool IsPredeclatation { get; private set; }
		public Type PropertyType { get; private set; }

		public PropertyDefinitionExpression(string propertyName, Type propertyType)
			: this(propertyName, propertyType, false)
		{	
		}

		public PropertyDefinitionExpression(string propertyName, Type propertyType, bool isPredeclatation)
		{
			this.PropertyType = propertyType;
			this.PropertyName = propertyName;
			this.IsPredeclatation = isPredeclatation;
		}
	}
}
