using System;
using System.Linq.Expressions;
using Dryice.Model;

namespace Dryice.Expressions
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
