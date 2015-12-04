using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class FickleExcludeControllerAttribute
		: Attribute
	{
		public Type Type { get; set; }
	
		public FickleExcludeControllerAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
