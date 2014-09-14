using System.Linq;

namespace Fickle.Dryfile
{
	public class UnexpectedDryfileTokenException
		: DryfileParserException
	{
		public DryfileToken Token { get; private set; }
		public object RelatedValue { get; private set; }
		public DryfileToken[] ExpectedTokens { get; private set; }

		public UnexpectedDryfileTokenException(DryfileToken token, object relatedValue, params DryfileToken[] expectedTokens)
			: base(string.Format("Expected ({0}) but was {1}", string.Join(",", expectedTokens.Select(c => c.ToString())), token))
		{
			this.Token = token;
			this.RelatedValue = relatedValue;
			this.ExpectedTokens = expectedTokens;
		}
	}
}
