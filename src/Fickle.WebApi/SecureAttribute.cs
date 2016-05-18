using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Method)]
	public class SecureAttribute
		: Attribute
	{
		public bool Secure { get; set; }

		public SecureAttribute(bool secure)
		{
			this.Secure = secure;
		}
	}
}
