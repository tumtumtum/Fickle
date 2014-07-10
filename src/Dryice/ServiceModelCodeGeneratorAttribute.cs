using System;

namespace Dryice
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ServiceModelCodeGeneratorAttribute
		: Attribute
	{
		public string[] Aliases { get; set; }

		public ServiceModelCodeGeneratorAttribute(params string[] aliases)
		{
			this.Aliases = aliases;
		}
	}
}
