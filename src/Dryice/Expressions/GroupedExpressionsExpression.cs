using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Dryice.Expressions
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

		public override System.Type Type
		{
			get
			{
				return this.Expressions[this.Expressions.Count - 1].Type;
			}
		}

		public virtual GroupedExpressionsExpressionStyle Style { get; set; }
		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public static GroupedExpressionsExpression FlatConcat(GroupedExpressionsExpressionStyle style, params Expression[] expressions)
		{
			var retval = new List<Expression>();

			foreach (var expression in expressions)
			{
				if (expression.NodeType == (ExpressionType)ServiceExpressionType.GroupedExpressions)
				{
					retval.AddRange(((GroupedExpressionsExpression)expression).Expressions);
				}
				else
				{
					retval.Add(expression);
				}
			}

			return new GroupedExpressionsExpression(retval, style);
		}

		public GroupedExpressionsExpression(Expression expression)
			: this(expression, GroupedExpressionsExpressionStyle.Narrow)
		{	
		}

		public GroupedExpressionsExpression(Expression expression, GroupedExpressionsExpressionStyle style)
			: this(new ReadOnlyCollection<Expression>(new List<Expression> { expression }), style)
		{
		}

		public GroupedExpressionsExpression(IEnumerable<Expression> expressions)
			: this(expressions, GroupedExpressionsExpressionStyle.Narrow)
		{
		}

		public GroupedExpressionsExpression(IEnumerable<Expression> expressions, GroupedExpressionsExpressionStyle style)
			: this(new ReadOnlyCollection<Expression>(expressions.ToList()), style)
		{
		}

		public GroupedExpressionsExpression(ReadOnlyCollection<Expression> expressions)
			: this(expressions, GroupedExpressionsExpressionStyle.Narrow)
		{	
		}

		public GroupedExpressionsExpression(ReadOnlyCollection<Expression> expressions, GroupedExpressionsExpressionStyle style)
		{
			this.Style = style; 
			this.Expressions = expressions;
		}
	}
}
