using System;
using System.Linq.Expressions;
using Dryice.Generators;

namespace Dryice.Expressions
{
	public class CodeLiteralExpression
		: Expression
	{
		public Action<SourceCodeGenerator> Action { get; private set; }

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.CodeLiteral;
			}
		}

		public CodeLiteralExpression(Action<SourceCodeGenerator> action)
		{
			this.Action = action;
		}
	}
}
