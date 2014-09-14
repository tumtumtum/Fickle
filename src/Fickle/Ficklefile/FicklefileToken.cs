namespace Fickle.Ficklefile
{
	public enum FicklefileToken
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
