using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class TypeDefinitionExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.TypeDefinition;
			}
		}

		public override Type Type
		{
			get
			{
				return type;
			}
		}
		private readonly Type type;

		public string Name
		{
			get
			{
				return this.type.Name;
			}
		}

		public bool IsEnumType
		{
			get { return this.type.BaseType == typeof(Enum); }
		}

		public bool IsPredeclaration { get; private set; }
		public ReadOnlyCollection<Type> InterfaceTypes { get; private set; }
		public Expression Header { get; private set; }
		public Expression Body { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes {get; private set;}

		public TypeDefinitionExpression(Type type, Expression header, Expression body, bool isPredeclaration, ReadOnlyDictionary<string, string> attributes = null, ReadOnlyCollection<Type> interfaceTypes = null)
		{
			this.type = type;
			this.Body = body;
			this.Header = header;
			this.Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
			this.IsPredeclaration = isPredeclaration;
			this.InterfaceTypes = interfaceTypes;
		}
	}
}
