﻿using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class IncludeExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.IncludeStatement;
			}
		}

		public string FileName { get; private set; }

		public IncludeExpression(string fileName)
		{
			this.FileName = fileName;
		}

		public static int Compare(IncludeExpression left, IncludeExpression right)
		{
			if (left.FileName.Length == right.FileName.Length)
			{
				return StringComparer.InvariantCulture.Compare(left.FileName, right.FileName);
			}
			else
			{
				return left.FileName.Length - right.FileName.Length;
			}
		}
	}
}
