using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class MethodDefinitionScope<T>
		: StatementBlockScope<T, MethodDefinitionScope<T>>
		where T : class
	{
		private readonly string name;
		private readonly Type returnType;

		public MethodDefinitionScope(string name, Type returnType, object parameters, T previousScope = default(T), Action<Expression> complete = null)
			: base(previousScope, complete)
		{
			this.name = name;
			this.returnType = returnType;
		}

		public MethodDefinitionScope(string name, string returnType, object parameters, T previousScope = default(T), Action<Expression> complete = null)
			: this(name, FickleType.Define(returnType), parameters, previousScope, complete)
		{
		}

		protected override Expression GetExpression()
		{
			return new MethodDefinitionExpression(this.name, new List<Expression>(), this.returnType, this.expressions.ToGroupedExpression().ToBlock(), false);
		}

		public virtual T EndMethod()
		{
			return base.End();
		}
	}
}