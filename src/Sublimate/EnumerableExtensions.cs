using System.Collections.Generic;
using System.Linq.Expressions;
using Sublimate.Expressions;

namespace Sublimate
{
	public static class EnumerableExtensions
	{
		public static GroupedExpressionsExpression ToGroupedExpression(this IEnumerable<Expression> enumerable, GroupedExpressionsExpressionStyle style = GroupedExpressionsExpressionStyle.Narrow)
		{
			return new GroupedExpressionsExpression(enumerable, style);
		}
	}
}
