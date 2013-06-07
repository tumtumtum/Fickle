using System.Collections.Generic;

namespace Sublimate.Generators.Objective
{
	public class ObjectiveLanguage
	{
		private static readonly string[] keywords =
		{
			"class",
			"interface",
			"property",
			"id",
			"protocol"
		};

		public virtual HashSet<string> Keywords
		{
			get
			{
				return new HashSet<string>(keywords);
			}
		}
	}
}
