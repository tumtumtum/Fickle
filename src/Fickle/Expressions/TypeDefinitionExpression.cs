using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class TypeDefinitionExpression
		: BaseExpression
	{
		public override Type Type { get; }
		public override ExpressionType NodeType => (ExpressionType)ServiceExpressionType.TypeDefinition;
		public bool IsEnumType => this.Type.BaseType == typeof(Enum);

		public Expression Header { get; private set; }
		public Expression Body { get; private set; }
		public bool IsPredeclaration { get; private set; }
		public ReadOnlyCollection<Type> InterfaceTypes { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes {get; private set;}

		public TypeDefinitionExpression(Type type, Expression header, Expression body, bool isPredeclaration, ReadOnlyDictionary<string, string> attributes = null, ReadOnlyCollection<Type> interfaceTypes = null)
		{
			this.Type = type;
			this.Body = body;
			this.Header = header;
			this.Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
			this.IsPredeclaration = isPredeclaration;
			this.InterfaceTypes = interfaceTypes;
		}
	}
}
