﻿using System;
using System.Linq.Expressions;
using Fickle.Generators;

namespace Fickle.Expressions
{
	public class CodeLiteralExpression
		: BaseExpression
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
