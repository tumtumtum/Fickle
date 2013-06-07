using System.Linq.Expressions;

namespace Sublimate.Expressions
{
	public class IncludeStatementExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.IncludeStatement;
			}
		}

		public string FileName { get; private set; }

		public IncludeStatementExpression(string fileName)
		{
			this.FileName = fileName;
		}
	}
}
