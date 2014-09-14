using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fickle
{
	public class DryDelegateType
		: DryType
	{
		public Type ReturnType { get; private set; }
		public ParameterInfo[] Parameters { get; private set; }

		private static string CreateTypeName(string delegateName, ParameterInfo[] parameters)
		{
			var builder = new StringBuilder();

			builder.Append(delegateName);
			builder.Append("<");
			builder.Append(String.Join(",", parameters.Select(c => c.Name).ToArray()));
			builder.Append(">");

			return builder.ToString();
		}

		public DryDelegateType(Type returnType, params ParameterInfo[] parameterTypes)
			: this("DryDelegate", returnType, parameterTypes)
		{
		}

		public DryDelegateType(string delegateName, Type returnType, params ParameterInfo[] parameterTypes)
			: base(CreateTypeName(delegateName, parameterTypes))
		{
			this.ReturnType = returnType;
			this.Parameters = parameterTypes;
		}
	}
}
