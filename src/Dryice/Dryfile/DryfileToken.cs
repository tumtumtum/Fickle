namespace Fickle.Dryfile
{
	public enum DryfileToken
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
