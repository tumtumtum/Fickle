using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Sublimate.Model;

namespace Sublimate.Expressions
{
	public class MethodDefinitionExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.MethodDefinition;
			}
		}

		public bool IsPredeclatation { get; private set; }
		public string Name { get; private set; }
		public string RawAttributes { get; private set; }
		public ServiceType ReturnType { get; private set; }
		public ReadOnlyCollection<Expression> Parameters { get; private set; }
 
		public MethodDefinitionExpression(string name, ReadOnlyCollection<Expression> parameters, ServiceType returnType)
			: this(name, parameters, returnType, false, null)
		{
		}

		public MethodDefinitionExpression(string name, ReadOnlyCollection<Expression> parameters, ServiceType returnType, bool isPredeclaration, string rawAttributes)
		{
			this.RawAttributes = rawAttributes;
			this.Name = name;
			this.ReturnType = returnType;
			this.Parameters = parameters;
			this.IsPredeclatation = isPredeclaration;
		}
	}
}
