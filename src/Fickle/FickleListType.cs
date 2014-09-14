using System;

namespace Fickle
{
	public class FickleListType
		: FickleBaseType
	{
		public Type ListElementType { get; private set; }

		public FickleListType(Type listElementType)
			: base("FickleListType")
		{
			this.ListElementType = listElementType;
		}
	}
}
