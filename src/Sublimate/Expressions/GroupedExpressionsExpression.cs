using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Sublimate.Expressions
{
	public class GroupedExpressionsExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.GroupedExpressions;
			}
		}

		public bool Isolated { get; private set; }
		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public GroupedExpressionsExpression(ReadOnlyCollection<Expression> expressions)
			: this(expressions, false)
		{	
		}

		public GroupedExpressionsExpression(ReadOnlyCollection<Expression> expressions, bool isolated)
		{
			this.Isolated = isolated; 
			this.Expressions = expressions;
		}
	}
}
