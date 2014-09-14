using System;

namespace Fickle.Ficklefile
{
	public class FicklefileParserException
		: Exception
	{
		public FicklefileParserException()
		{
		}

		public FicklefileParserException(string message)
			: base(message)
		{	
		}
	}
}
