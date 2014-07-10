﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dryice.Expressions
{
	public class CommentExpression
		: Expression
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
