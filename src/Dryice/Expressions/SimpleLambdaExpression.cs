using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class SimpleLambdaExpression
		: BaseExpression
	{
		public Type ReturnType { get; private set; }
		public Expression Body { get; private set; }
		public ReadOnlyCollection<Expression> Variables { get; private set; }
		public ReadOnlyCollection<Expression> Parameters { get; private set; }

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.SimpleLambdaExpression;
			}
		}

		public override Type Type
		{
			get
			{
				if (type == null)
				{
					var i = 0;

					type = new DryDelegateType(this.ReturnType ?? this.Body.Type, this.Parameters.Select(c => new DryParameterInfo(c.Type, "arg" + i++)).ToArray());
				}

				return type;
			}
		}

		private Type type;

		public SimpleLambdaExpression(Type returnType, Expression body, IEnumerable<Expression> variables, IEnumerable<Expression> parameters)
		{
			this.ReturnType = returnType;
			this.Body = body;
			this.Variables = variables.ToReadOnlyCollection();
			this.Parameters = parameters.ToReadOnlyCollection<Expression>();
		}

		public SimpleLambdaExpression(Type returnType, Expression body, IEnumerable<Expression> variables, params Expression[] parameters)
		{
			this.ReturnType = returnType;
			this.Body = body;
			this.Variables = variables.ToReadOnlyCollection();
			this.Parameters = parameters.ToReadOnlyCollection();
		}
	}
}
