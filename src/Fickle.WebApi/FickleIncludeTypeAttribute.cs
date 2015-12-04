using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class FickleIncludeTypeAttribute
		: Attribute
	{
		public Type Type { get; set; }
		public bool IncludeRelatives { get; set; }
		
		public FickleIncludeTypeAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
