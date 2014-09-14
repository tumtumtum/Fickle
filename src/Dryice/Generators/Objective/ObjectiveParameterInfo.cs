using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fickle.Generators.Objective
{
	public class ObjectiveParameterInfo
		: DryParameterInfo
	{
		public bool IsCStyleParameter { get; set; }

		public ObjectiveParameterInfo(Type parameterType, string name, bool isCStyleParameter = false)
			: base(parameterType, name)
		{
			this.IsCStyleParameter = isCStyleParameter;
		}
	}
}
