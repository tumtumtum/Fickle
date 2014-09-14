using System.Linq;

namespace Fickle.Ficklefile
{
	public class UnexpectedFicklefileTokenException
		: FicklefileParserException
	{
		public FicklefileToken Token { get; private set; }
		public object RelatedValue { get; private set; }
		public FicklefileToken[] ExpectedTokens { get; private set; }

		public UnexpectedFicklefileTokenException(FicklefileToken token, object relatedValue, params FicklefileToken[] expectedTokens)
			: base(string.Format("Expected ({0}) but was {1}", string.Join(",", expectedTokens.Select(c => c.ToString())), token))
		{
			this.Token = token;
			this.RelatedValue = relatedValue;
			this.ExpectedTokens = expectedTokens;
		}
	}
}
