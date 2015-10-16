using System;
using System.Text;

namespace Fickle.Generators.Objective
{
	public static class ObjectiveStringExtensions
	{
		public static string ToCamelCase(this string value)
		{
			var i = 0;
			var nextIsCaps = false;
			var retval = new StringBuilder();

			foreach (var c in value)
			{
				if (c != '_')
				{
					retval.Append(i == 0 && !nextIsCaps ? Char.ToLower(c) : nextIsCaps ? Char.ToUpper(c) : c);
				}

				nextIsCaps = c == '_';

				i++;
			}

			return retval.ToString();
		}
	}
}
