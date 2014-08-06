using System;

namespace Dryice
{
	public class DryNullable
		: DryBaseType
	{
		public Type UnderlyingType { get; private set; }

		public static Type GetUnderlyingType(Type type)
		{
			var retval = Nullable.GetUnderlyingType(type);

			if (retval != null)
			{
				return retval;
			}

			var nullable = type as DryNullable;

			if (nullable == null)
			{
				return null;
			}

			return nullable.UnderlyingType;
		}

		public DryNullable(Type underlyingType)
			: base("DryNullable")
		{
			this.UnderlyingType = underlyingType;
		}

		public override string ToString()
		{
			return string.Format("{0}<{1}>", this.Name, this.UnderlyingType);
		}
	}
}
