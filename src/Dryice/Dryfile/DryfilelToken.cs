namespace Dryice.Dryfile
{
	public enum DryfilelToken
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
