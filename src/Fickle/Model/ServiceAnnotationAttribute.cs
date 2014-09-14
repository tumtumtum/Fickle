using System;

namespace Fickle.Model
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ServiceAnnotationAttribute
		: Attribute
	{
	}
}
