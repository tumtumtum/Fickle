using System;

namespace Sublimate
{
	public class SublimateListType
		: SublimateBasicType
	{
		public Type ListElementType { get; set; }

		public SublimateListType(Type listElementType)
			: base("SublimateListType")
		{
			this.ListElementType = listElementType;
		}
	}
}
