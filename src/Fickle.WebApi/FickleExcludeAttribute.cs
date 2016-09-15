using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class FickleExcludeAttribute : Attribute
	{
	}
}