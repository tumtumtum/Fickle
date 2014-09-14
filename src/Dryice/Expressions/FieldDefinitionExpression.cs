using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class FieldDefinitionExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.FieldDefinition;
			}
		}

		public override Type Type { get { return typeof(void); } }

		public FieldDefinitionExpression(string propertyName, Type propertyType, AccessModifiers accessModifiers = AccessModifiers.Public)
			: this(propertyName, propertyType, false, accessModifiers)
		{	
		}

		public FieldDefinitionExpression(string propertyName, Type propertyType, bool isPredeclaration, AccessModifiers accessModifiers = AccessModifiers.Public)
		{
			this.PropertyType = propertyType;
			this.PropertyName = propertyName;
			this.IsPredeclaration = isPredeclaration;
			this.AccessModifiers = accessModifiers;
		}

		public string PropertyName { get; private set; }
		public bool IsPredeclaration { get; private set; }
		public Type PropertyType { get; private set; }
		public AccessModifiers AccessModifiers { get; private set; }
	}
}
