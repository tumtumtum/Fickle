using System;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class ParameterDefinitionExpression
		: Expression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.ParameterDefinition;
			}
		}

		public int Index { get; private set; }
		public string ParameterName { get; private set; }
		public Type ParameterType { get; private set; }

		public ParameterDefinitionExpression(ParameterExpression parameterExpression, int index)
			: this(parameterExpression.Name, parameterExpression.Type, index)
		{
		}

		public ParameterDefinitionExpression(string parameterName, Type parameterType, int index)
		{
			this.Index = index;
			this.ParameterName = parameterName;
			this.ParameterType = parameterType;
		}
	}
}
