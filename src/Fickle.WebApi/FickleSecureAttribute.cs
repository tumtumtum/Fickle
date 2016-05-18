using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
	public class FickleSecureAttribute
		: Attribute
	{
		public bool Secure { get; set; }

		public FickleSecureAttribute(bool secure)
		{
			this.Secure = secure;
		}
	}
}
