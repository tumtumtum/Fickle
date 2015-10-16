using System;

namespace Fickle.Generators.Objective
{
	public class ObjectiveParameterInfo
		: FickleParameterInfo
	{
		public bool IsCStyleParameter { get; set; }

		public ObjectiveParameterInfo(Type parameterType, string name, bool isCStyleParameter = false)
			: base(parameterType, name)
		{
			this.IsCStyleParameter = isCStyleParameter;
		}
	}
}
