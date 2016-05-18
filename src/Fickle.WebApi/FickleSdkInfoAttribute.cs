using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class FickleSdkInfoAttribute
		: Attribute
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public string ServiceNameSuffix { get; set; }

		public bool SecureByDefault { get; set; }
	}
}
