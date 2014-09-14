using System;

namespace Fickle
{
	public class FickleNullable
		: FickleBaseType
	{
		public Type UnderlyingType { get; private set; }

		public static Type GetUnderlyingType(Type type)
		{
			var retval = Nullable.GetUnderlyingType(type);

			if (retval != null)
			{
				return retval;
			}

			var nullable = type as FickleNullable;

			if (nullable == null)
			{
				return null;
			}

			return nullable.UnderlyingType;
		}

		public FickleNullable(Type underlyingType)
			: base("FickleNullable")
		{
			this.UnderlyingType = underlyingType;
		}

		public override string ToString()
		{
			return string.Format("{0}<{1}>", this.Name, this.UnderlyingType);
		}
	}
}
