using System.Linq.Expressions;
using Sublimate.Model;

namespace Sublimate.Expressions
{
	public class ReferencedTypeExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.ReferencedType;
			}
		}

		public ServiceType ReferencedType { get; private set; }

		public ReferencedTypeExpression(ServiceType referencedType)
		{
			this.ReferencedType = referencedType;
		}
	}
}
