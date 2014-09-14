using System;

namespace Fickle.Dryfile
{
	public class DryfileParserException
		: Exception
	{
		public DryfileParserException()
		{
		}

		public DryfileParserException(string message)
			: base(message)
		{	
		}
	}
}
