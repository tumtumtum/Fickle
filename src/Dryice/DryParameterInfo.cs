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

		private Type parameterType;

		public override string Name
		{
			get
			{
				return name;
			}
		}

		private string name;
	}
}
