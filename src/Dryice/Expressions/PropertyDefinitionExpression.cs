using System;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public  class PropertyDefinitionExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.PropertyDefinition;
			}
		}

		public override Type Type { get { return typeof(void); } }

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

		public string PropertyName { get; private set; }
		public bool IsPredeclatation { get; private set; }
		public Type PropertyType { get; private set; }
	}
}
