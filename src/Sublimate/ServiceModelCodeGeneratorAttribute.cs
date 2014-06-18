using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sublimate
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
