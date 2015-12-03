using System;

namespace Fickle
{
	public class UniqueNameMaker
	{
		private readonly Func<string, bool> nameExists;

		public UniqueNameMaker(Func<string, bool> nameExists)
		{
			this.nameExists = nameExists;
		}

		public virtual string Make(string name)
		{
			while (nameExists(name))
			{
				name = "_" + name;
			}

			return name;
		}
	}
}
