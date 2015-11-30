using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public  class PropertyDefinitionExpression
		: BaseExpression
	{
		public override Type Type => typeof(void);
		public override ExpressionType NodeType => (ExpressionType)ServiceExpressionType.PropertyDefinition;
		public string PropertyName { get; private set; }
		public bool IsPredeclaration { get; private set; }
		public Type PropertyType { get; private set; }
		public IReadOnlyList<string> Modifiers { get; private set; }

		public PropertyDefinitionExpression(string propertyName, Type propertyType)
			: this(propertyName, propertyType, false, new string[0])
		{	
		}

		public PropertyDefinitionExpression(string propertyName, Type propertyType, bool isPredeclaration)
			: this(propertyName, propertyType, isPredeclaration, new string[0])
		{
		}

        public PropertyDefinitionExpression(string propertyName, Type propertyType, bool isPredeclaration, IEnumerable<string> modifiers)
		{
			this.PropertyType = propertyType;
			this.PropertyName = propertyName;
			this.IsPredeclaration = isPredeclaration;
			this.Modifiers = new ReadOnlyCollection<string>(new List<string>(modifiers));
		}
	}
}
