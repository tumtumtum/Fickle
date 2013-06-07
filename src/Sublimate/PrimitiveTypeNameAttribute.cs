using System;

namespace Sublimate
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class PrimitiveTypeNameAttribute
		: Attribute
	{
		public Type Type { get; set; }
		public string Name { get; set; }
		public bool IsReferenceType { get; set; }
		
		public PrimitiveTypeNameAttribute(Type type, string name, bool isReferenceType)
		{
			this.Name = name;
			this.Type = type;
			this.IsReferenceType = isReferenceType;
		}
	}
}
