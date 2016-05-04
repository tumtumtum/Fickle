using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fickle.Ficklefile
{
	public class FicklefileTokenizer
	{
		private int currentChar;
		private int workingIndent;
		private bool encounteredSymbolOnCurrentLine;
		private readonly StringBuilder stringBuilder;
		private readonly Stack<int> indentStack; 
		public TextReader Reader { get; set; }
		public FicklefileKeyword CurrentKeyword { get; private set; }
		public FicklefileToken CurrentToken { get; private set; }
		public string CurrentString { get; private set; }
		public long CurrentInteger { get; private set; }
		public double CurrentFloat { get; private set; }
		private readonly Dictionary<string, FicklefileKeyword> keywordsByName = new Dictionary<string, FicklefileKeyword>(StringComparer.InvariantCultureIgnoreCase);
		
		public FicklefileTokenizer(TextReader reader)
		{
			this.currentChar = -1;
			this.Reader = reader;
			this.CurrentToken = FicklefileToken.None;
			this.stringBuilder = new StringBuilder();

			foreach (var name in Enum.GetNames(typeof(FicklefileKeyword)))
			{
				this.keywordsByName[name] = (FicklefileKeyword)Enum.Parse(typeof(FicklefileKeyword), name);
			}

			this.indentStack = new Stack<int>();

			this.indentStack.Push(0);

			this.ConsumeChar();
		}

		private int CurrentIndent
		{
			get
			{
				return this.indentStack.Peek();
			}
			set
			{
				this.indentStack.Pop();
				this.indentStack.Push(value);
			}
		}
		
		public string CurrentIdentifier
		{
			get
			{
				return this.CurrentString;
			}
		}

		public string CurrentTypeName
		{
			get
			{
				return this.CurrentString;
			}
		}

		public object CurrentValue
		{
			get
			{
				switch (this.CurrentToken)
				{
					case FicklefileToken.Colon:
						return ':';
					case FicklefileToken.EndOfFile:
						return null;
					case FicklefileToken.Float:
						return this.CurrentFloat;
					case FicklefileToken.Integer:
						return this.CurrentInteger;
					case FicklefileToken.Keyword:
						return this.CurrentKeyword;
					case FicklefileToken.None:
						return null;
					case FicklefileToken.StringLiteral:
						return this.CurrentString;
					case FicklefileToken.Identifier:
						return this.CurrentString;
					default:
						return null;
				}	
			}
		}

		private void ConsumeChar()
		{
			this.currentChar = this.Reader.Read();
		}

		public void ReadStringToEnd()
		{
			var nonWhitespaceEncountered = false;
			var builder = new StringBuilder();

			while (true)
			{
				if (this.currentChar == -1)
				{
					break;
				}
				else if (this.currentChar == '\n')
				{
					this.ConsumeChar();

					this.encounteredSymbolOnCurrentLine = false;
					this.workingIndent = 0;

					break;
				}
				else if (this.currentChar == '\r')
				{
					this.ConsumeChar();

					continue;
				}

				if (this.currentChar == ' ' || this.currentChar == '\t' && !nonWhitespaceEncountered)
				{
					this.ConsumeChar();

					continue;
				}

				nonWhitespaceEncountered = true;

				builder.Append((char)this.currentChar);

				this.ConsumeChar();
			}

			this.CurrentToken = FicklefileToken.StringLiteral;
			this.CurrentString = builder.ToString();
		}

		public FicklefileToken ReadNextToken()
		{
			if (this.CurrentToken == FicklefileToken.Dedent)
			{
				if (this.workingIndent != this.CurrentIndent)
				{
					this.indentStack.Pop();

					return this.CurrentToken;
				}
				else
				{
					this.workingIndent = 0;
				}
			}

			while (char.IsWhiteSpace((char)this.currentChar))
			{
				if (this.currentChar == '\n')
				{
					this.encounteredSymbolOnCurrentLine = false;
					this.workingIndent = 0;
				}
				else if (this.currentChar == ' ' || this.currentChar == '\t')
				{
					if (!this.encounteredSymbolOnCurrentLine)
					{
						this.workingIndent += this.currentChar == '\t' ? 4 : 1;
					}
				}

				this.ConsumeChar();
			}

			if (this.currentChar == -1)
			{
				if (this.indentStack.Count > 1)
				{
					this.indentStack.Pop();
					this.workingIndent = this.indentStack.Peek();

					this.CurrentToken = FicklefileToken.Dedent;

					return this.CurrentToken;
				}
				else
				{
					this.CurrentToken = FicklefileToken.EndOfFile;

					return this.CurrentToken;
				}
			}

			if (!this.encounteredSymbolOnCurrentLine)
			{
				this.encounteredSymbolOnCurrentLine = true;

				if (this.CurrentIndent != this.workingIndent)
				{
					if (this.workingIndent > this.CurrentIndent)
					{
						this.indentStack.Push(this.workingIndent);

						this.CurrentToken = FicklefileToken.Indent;

						return this.CurrentToken;
					}
					else
					{
						this.indentStack.Pop();

						this.CurrentToken = FicklefileToken.Dedent;

						return this.CurrentToken;
					}
				}
			}

			if (this.currentChar == ':')
			{
				this.ConsumeChar();
				this.CurrentToken = FicklefileToken.Colon;
			}
			else if (this.currentChar == '?')
			{
				this.ConsumeChar();
				this.CurrentToken = FicklefileToken.QuestionMark;
			}
			else if (this.currentChar == '[')
			{
				this.ConsumeChar(); 
				this.CurrentToken = FicklefileToken.OpenBracket;
			}
			else if (this.currentChar == ']')
			{
				this.ConsumeChar(); 
				this.CurrentToken = FicklefileToken.CloseBracket;
			}
			else if (this.currentChar == '(')
			{
				this.ConsumeChar();
				this.CurrentToken = FicklefileToken.OpenParen;
			}
			else if (this.currentChar == ')')
			{
				this.ConsumeChar();
				this.CurrentToken = FicklefileToken.CloseParen;
			}
			else if (char.IsDigit((char)this.currentChar) || (char)this.currentChar == '-')
			{
				var foundPoint = false;
				this.stringBuilder.Clear();

				if ((char)this.currentChar == '-')
				{
					this.stringBuilder.Append((char)this.currentChar);
					this.ConsumeChar();
				}

				while (this.currentChar != -1 && (char.IsDigit((char)this.currentChar) || (this.currentChar == '.' && !foundPoint)))
				{
					if (this.currentChar == '.')
					{
						foundPoint = true;
					}

					this.stringBuilder.Append((char)this.currentChar);

					this.ConsumeChar();
				}

				if (foundPoint)
				{
					this.CurrentToken = FicklefileToken.Float;
					this.CurrentFloat = Double.Parse(this.stringBuilder.ToString());
				}
				else
				{
					this.CurrentToken = FicklefileToken.Integer;
					this.CurrentInteger = Int64.Parse(this.stringBuilder.ToString());
				}
			}
			else if ((char)this.currentChar == '@' || (char)this.currentChar == '$' || (char)this.currentChar == '^' || char.IsLetter((char)this.currentChar) || this.currentChar == '_')
			{
				var isIdentifier = this.currentChar == '$';
				var isAnnotation = this.currentChar == '@';
				var isLiteralIdentifier = this.currentChar == '^';

				if (isAnnotation || isLiteralIdentifier)
				{
					this.ConsumeChar();
				}

				this.stringBuilder.Clear();

				while (this.currentChar != -1 && (char.IsLetterOrDigit((char)this.currentChar) || (char)this.currentChar == '-') || this.currentChar == '_')
				{
					this.stringBuilder.Append((char)this.currentChar);

					this.ConsumeChar();
				}

				this.CurrentString = this.stringBuilder.ToString();

				FicklefileKeyword keyword;

				if (isAnnotation)
				{
					this.CurrentToken = FicklefileToken.Annotation;
				}
				else if (!isIdentifier && this.keywordsByName.TryGetValue(this.CurrentString, out keyword))
				{
					this.CurrentKeyword = keyword;
					this.CurrentToken = FicklefileToken.Keyword;
				}
				else
				{
					this.CurrentToken = FicklefileToken.Identifier;
				}
			}
			else
			{
				throw new InvalidOperationException("Unexpected char: '" + (char)this.currentChar + "'");
			}

			return this.CurrentToken;
		}
	}
}
