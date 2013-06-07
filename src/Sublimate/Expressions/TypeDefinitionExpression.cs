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

		public bool IsPredeclaration { get; private set; }
		public string Name { get; private set; }
		public ServiceType BaseType { get; private set; }
		public ReadOnlyCollection<ServiceType> InterfaceTypes { get; private set; }
		public Expression Header { get; private set; }
		public Expression Body { get; private set; }

		public TypeDefinitionExpression(Expression header, Expression body, string name, ServiceType baseType)
			: this(header, body, name, baseType, false, null)
		{
		}

		public TypeDefinitionExpression(Expression header, Expression body, string name, ServiceType baseType, bool isPredeclaration, ReadOnlyCollection<ServiceType> interfaceTypes)
		{
			this.Name = name;
			this.Body = body;
			this.Header = header;
			this.BaseType = baseType;
			this.IsPredeclaration = isPredeclaration;
			this.InterfaceTypes = interfaceTypes;
		}
	}
}
