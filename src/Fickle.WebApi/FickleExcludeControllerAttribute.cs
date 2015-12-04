using System;

namespace Fickle.WebApi
{
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
