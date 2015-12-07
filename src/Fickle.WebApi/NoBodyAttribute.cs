using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Method)]
	public class NoBodyAttribute
		: Attribute
	{
	}
}
