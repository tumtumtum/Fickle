using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sublimate.Webs
{
	public class UnexpectedWebsTokenException
		: WebsParserException
	{
		public WebsToken Token { get; private set; }
		public object RelatedValue { get; private set; }
		public WebsToken[] ExpectedTokens { get; set; }

		public UnexpectedWebsTokenException(WebsToken token, object relatedValue, params WebsToken[] expectedTokens)
		{
			this.Token = token;
			this.RelatedValue = relatedValue;
			ExpectedTokens = expectedTokens;
		}
	}
}
