using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class TypeDefinitionExpression
		: Expression
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

		public Type BaseType { get; private set; }
		
		public string Name
		{
			get
			{
				return this.type.Name;
			}
		}

		public bool IsPredeclaration { get; private set; }
		public ReadOnlyCollection<Type> InterfaceTypes { get; private set; }
		public Expression Header { get; private set; }
		public Expression Body { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes {get; private set;}

		public TypeDefinitionExpression(Type type, Type baseType, Expression header, Expression body, bool isPredeclaration, ReadOnlyDictionary<string, string> attributes = null, ReadOnlyCollection<Type> interfaceTypes = null)
		{
			this.type = type;
			this.Body = body;
			this.Header = header;
			this.BaseType = baseType;
			this.Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
			this.IsPredeclaration = isPredeclaration;
			this.InterfaceTypes = interfaceTypes;
		}
	}
}
