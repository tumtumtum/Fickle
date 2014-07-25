using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dryice.Generators.Java
{
	public class JavaParameterInfo
		: DryParameterInfo
	{
		public bool IsCStyleParameter { get; set; }

		public JavaParameterInfo(Type parameterType, string name, bool isCStyleParameter = false)
			: base(parameterType, name)
		{
			this.IsCStyleParameter = isCStyleParameter;
		}
	}
}
