using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

		public override System.Type Type
		{
			get
			{
				return this.Expressions[this.Expressions.Count - 1].Type;
			}
		}

		public bool Isolated { get; private set; }
		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public GroupedExpressionsExpression(Expression expression)
			: this(expression, false)
		{	
		}

		public GroupedExpressionsExpression(Expression expression, bool isolated)
			: this(new ReadOnlyCollection<Expression>(new List<Expression> { expression }), isolated)
		{
		}

		public GroupedExpressionsExpression(IEnumerable<Expression> expressions)
			: this(expressions, false)
		{
		}

		public GroupedExpressionsExpression(IEnumerable<Expression> expressions, bool isolated)
			: this(new ReadOnlyCollection<Expression>(expressions.ToList()), isolated)
		{
		}

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
