using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class BaseExpression
		: Expression
	{
		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}
