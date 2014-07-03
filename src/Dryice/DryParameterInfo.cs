using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dryice
{
	public class DryParameterInfo
		: ParameterInfo
	{
		public DryParameterInfo(Type parameterType, string name)
		{
			this.name = name;
			this.parameterType = parameterType;
		}

		public override Type ParameterType
		{
			get
			{
				return parameterType;
			}
		}
		private readonly Type parameterType;

		public override string Name
		{
			get
			{
				return name;
			}
		}
		private readonly string name;
	}
}
