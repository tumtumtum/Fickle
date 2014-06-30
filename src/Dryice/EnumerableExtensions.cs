using System.Collections.Generic;
using System.Linq.Expressions;
using Dryice.Expressions;

namespace Dryice
{
	public static class EnumerableExtensions
	{
		public static GroupedExpressionsExpression ToGroupedExpression(this IEnumerable<Expression> enumerable, GroupedExpressionsExpressionStyle style = GroupedExpressionsExpressionStyle.Narrow)
		{
			return new GroupedExpressionsExpression(enumerable, style);
		}
	}
}
