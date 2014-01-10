using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Sublimate.Model;

namespace Sublimate.Expressions
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
		private Type type;

		public Type BaseType { get; private set; }
		
		public string Name
		{
			get
			{
				return this.type.Name;
			}
		}

		public bool IsPredeclaration { get; private set; }
		public ReadOnlyCollection<ServiceClass> InterfaceTypes { get; private set; }
		public Expression Header { get; private set; }
		public Expression Body { get; private set; }

		public TypeDefinitionExpression(Type type, Type baseType, Expression header, Expression body)
			: this(type, baseType, header, body, false, null)
		{
		}

		public TypeDefinitionExpression(Type type, Type baseType, Expression header, Expression body, bool isPredeclaration, ReadOnlyCollection<ServiceClass> interfaceTypes)
		{
			this.type = type;
			this.Body = body;
			this.Header = header;
			this.BaseType = baseType;
			this.IsPredeclaration = isPredeclaration;
			this.InterfaceTypes = interfaceTypes;
		}
	}
}
