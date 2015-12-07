using System;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class ElseScope<T>
		: StatementBlockScope<T, ElseScope<T>>
		where T : class
	{
		public ElseScope(T ifElseScope, Action<Expression> complete)
			: base(ifElseScope, complete)
		{
		}

		public virtual T EndIf() => base.End();
	}
}