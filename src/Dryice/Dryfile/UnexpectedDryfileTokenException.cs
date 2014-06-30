using System.Linq;

namespace Dryice.Dryfile
{
	public class UnexpectedDryfileTokenException
		: DryfileParserException
	{
		public DryfilelToken Token { get; private set; }
		public object RelatedValue { get; private set; }
		public DryfilelToken[] ExpectedTokens { get; set; }

		public UnexpectedDryfileTokenException(DryfilelToken token, object relatedValue, params DryfilelToken[] expectedTokens)
			: base(string.Format("Expected ({0}) but was {1}", string.Join(",", expectedTokens.Select(c => c.ToString())), token))
		{
			this.Token = token;
			this.RelatedValue = relatedValue;
			this.ExpectedTokens = expectedTokens;
		}
	}
}
