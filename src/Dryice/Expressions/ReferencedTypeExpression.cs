using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class ReferencedTypeExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.ReferencedType;
			}
		}

		public Type ReferencedType { get; private set; }

		public ReferencedTypeExpression(Type referencedType)
		{
			this.ReferencedType = referencedType;
		}
	}
}
