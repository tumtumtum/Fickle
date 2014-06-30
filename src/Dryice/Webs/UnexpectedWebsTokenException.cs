using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dryice.Webs
{
	public class UnexpectedWebsTokenException
		: WebsParserException
	{
		public WebsToken Token { get; private set; }
		public object RelatedValue { get; private set; }
		public WebsToken[] ExpectedTokens { get; set; }

		public UnexpectedWebsTokenException(WebsToken token, object relatedValue, params WebsToken[] expectedTokens)
			: base(string.Format("Expected ({0}) but was {1}", string.Join(",", expectedTokens.Select(c => c.ToString())), token))
		{
			this.Token = token;
			this.RelatedValue = relatedValue;
			ExpectedTokens = expectedTokens;
		}
	}
}
