using System;

namespace Fickle.WebApi
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class FickleIncludeTypeAttribute
		: Attribute
	{
		/// <summary>
		/// The type to include in the Fickle service model
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Whether to include other types from the assembly that are in the same namespace as the target type
		/// </summary>
		public bool IncludeRelatives { get; set; }
		
		public FickleIncludeTypeAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
