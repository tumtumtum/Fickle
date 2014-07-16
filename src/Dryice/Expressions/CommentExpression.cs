using System;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class CommentExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.Comment;
			}
		}

		public override Type Type
		{
			get
			{
				return typeof(void);
			}
		}

		public string Comment { get; private set; }

		public CommentExpression(string comment)
		{
			this.Comment = comment;
		}
	}
}
