using System;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class NamespaceExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.Namespace;
			}
		}

		public string NameSpace { get; private set; }
		public Expression Header { get; private set; }
		public Expression Body { get; private set; }

		public NamespaceExpression(string namespaceName)
		{
			this.NameSpace = namespaceName;
		}

		public NamespaceExpression(string namespaceName, Expression header, Expression body)
		{
			this.NameSpace = namespaceName;
			this.Header = header;
			this.Body = body;
		}

		public static int Compare(NamespaceExpression left, NamespaceExpression right)
		{
			if (left.NameSpace.Length == right.NameSpace.Length)
			{
				return StringComparer.InvariantCulture.Compare(left.NameSpace, right.NameSpace);
			}
			else
			{
				return left.NameSpace.Length - right.NameSpace.Length;
			}
		}
	}
}
