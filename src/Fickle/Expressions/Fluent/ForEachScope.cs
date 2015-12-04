using System;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ForEachScope<T>
		: StatementBlockScope<T, ForEachScope<T>>
		where T : class
	{
		public ForEachScope(T previousScope, Action<Expression> complete)
			: base(previousScope, complete)
		{
		}

		public virtual T EndForEach() => base.End();
	}
}