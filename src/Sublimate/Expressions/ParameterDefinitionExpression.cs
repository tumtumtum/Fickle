using System.Linq.Expressions;
using Sublimate.Model;

namespace Sublimate.Expressions
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
		public ServiceType ParameterType { get; private set; }
		
		public ParameterDefinitionExpression(string parameterName, ServiceType parameterType, int index)
		{
			this.Index = index;
			this.ParameterName = parameterName;
			this.ParameterType = parameterType;
		}
	}
}
