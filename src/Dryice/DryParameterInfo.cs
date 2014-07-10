using System;
using System.Reflection;

namespace Dryice
{
	public class DryParameterInfo
		: ParameterInfo
	{
		private readonly string name;
		private readonly Type parameterType;

		public override string Name { get { return name; } }
		public override Type ParameterType { get { return parameterType; } }

		public DryParameterInfo(Type parameterType, string name)
		{
			this.name = name;
			this.parameterType = parameterType;
		}
	}
}
