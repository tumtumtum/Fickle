using System;
using Sublimate.Model;

namespace Sublimate
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PrimitiveTypeNameAttribute
		: Attribute
	{
		public string Name { get; set; }
		public PrimitiveType PrimitiveType { get; set; }
		
		public PrimitiveTypeNameAttribute(PrimitiveType primitiveType, string name)
		{
			this.Name = name;
			this.PrimitiveType = primitiveType;
		}
	}
}
