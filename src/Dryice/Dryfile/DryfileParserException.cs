using System;

namespace Dryice.Dryfile
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
