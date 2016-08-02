using System;

namespace Fickle
{
	public class FickleAttributedType
		: FickleType
	{
		public Type Type { get; }
		public bool Modifiable { get; }

		public FickleAttributedType(Type type, bool modifiable)
			: base(type.Name)
		{
			this.Type = type;
			this.Modifiable = modifiable;
		}
	}
}