using System;

namespace Fickle.Generators
{
	[Flags]
	public enum BraceLanguageStyleIndentationOptions
	{
		Default = 0,
		NewLineAfter = 1,
		IncludeBraces = 2,
		IncludeBracesNewLineAfter = NewLineAfter | IncludeBraces 
	}
}
