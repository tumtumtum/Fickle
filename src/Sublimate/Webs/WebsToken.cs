using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sublimate.Webs
{
	public enum WebsToken
	{
		None,
		Keyword,
		Annotation,
		Integer,
		Float,
		Identifier,
		Colon,
		QuestionMark,
		OpenBracket,
		CloseBracket,
		OpenParen,
		CloseParen,
		StringLiteral,
		Indent,
		Dedent,
		EndOfFile
	}
}
