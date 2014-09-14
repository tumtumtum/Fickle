using System.Linq.Expressions;

namespace Fickle.Expressions
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
