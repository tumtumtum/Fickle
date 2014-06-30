using System;

namespace Dryice
{
	public class DryiceListType
		: DryiceBasicType
	{
		public Type ListElementType { get; set; }

		public DryiceListType(Type listElementType)
			: base("DryiceListType")
		{
			this.ListElementType = listElementType;
		}
	}
}
