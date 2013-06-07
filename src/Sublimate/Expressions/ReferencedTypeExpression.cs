using System;
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

		public Type ReferencedType { get; private set; }

		public ReferencedTypeExpression(Type referencedType)
		{
			this.ReferencedType = referencedType;
		}
	}
}
