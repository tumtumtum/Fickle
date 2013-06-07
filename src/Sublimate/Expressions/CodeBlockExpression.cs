using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Sublimate.Expressions
{
	public class CodeBlockExpression
		: GroupedExpressionsExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.CodeBlock;
			}
		}

		public CodeBlockExpression(ReadOnlyCollection<Expression> expressions)
			: base(expressions)
		{
		}
	}
}
