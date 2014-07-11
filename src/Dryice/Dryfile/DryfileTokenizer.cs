using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dryice.Dryfile
{
	public class DryfileTokenizer
	{
		private int currentChar;
		private int workingIndent;
		private bool encounteredSymbolOnCurrentLine;
		private readonly StringBuilder stringBuilder;
		private readonly Stack<int> indentStack; 
		public TextReader Reader { get; set; }
		public DryfileKeyword CurrentKeyword { get; private set; }
		public DryfileToken CurrentToken { get; private set; }
		public string CurrentString { get; private set; }
		public long CurrentInteger { get; private set; }
		public double CurrentFloat { get; private set; }
		private readonly Dictionary<string, DryfileKeyword> keywordsByName = new Dictionary<string, DryfileKeyword>(StringComparer.InvariantCultureIgnoreCase);
		
		public DryfileTokenizer(TextReader reader)
		{
			this.currentChar = -1;
			this.Reader = reader;
			this.CurrentToken = DryfileToken.None;
			this.stringBuilder = new StringBuilder();

			foreach (var name in Enum.GetNames(typeof(DryfileKeyword)))
			{
				this.keywordsByName[name] = (DryfileKeyword)Enum.Parse(typeof(DryfileKeyword), name);
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
					case DryfileToken.Colon:
						return ':';
					case DryfileToken.EndOfFile:
						return null;
					case DryfileToken.Float:
						return this.CurrentFloat;
					case DryfileToken.Integer:
						return this.CurrentInteger;
					case DryfileToken.Keyword:
						return this.CurrentKeyword;
					case DryfileToken.None:
						return null;
					case DryfileToken.StringLiteral:
						return this.CurrentString;
					case DryfileToken.Identifier:
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

			this.CurrentToken = DryfileToken.StringLiteral;
			this.CurrentString = builder.ToString();
		}

		public DryfileToken ReadNextToken()
		{
			if (this.CurrentToken == DryfileToken.Dedent)
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

					this.CurrentToken = DryfileToken.Dedent;

					return this.CurrentToken;
				}
				else
				{
					this.CurrentToken = DryfileToken.EndOfFile;

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

						this.CurrentToken = DryfileToken.Indent;

						return this.CurrentToken;
					}
					else
					{
						this.indentStack.Pop();

						this.CurrentToken = DryfileToken.Dedent;

						return this.CurrentToken;
					}
				}
			}

			if (this.currentChar == ':')
			{
				this.ConsumeChar();
				this.CurrentToken = DryfileToken.Colon;
			}
			else if (this.currentChar == '?')
			{
				this.ConsumeChar();
				this.CurrentToken = DryfileToken.QuestionMark;
			}
			else if (this.currentChar == '[')
			{
				this.ConsumeChar(); 
				this.CurrentToken = DryfileToken.OpenBracket;
			}
			else if (this.currentChar == ']')
			{
				this.ConsumeChar(); 
				this.CurrentToken = DryfileToken.CloseBracket;
			}
			else if (this.currentChar == '(')
			{
				this.ConsumeChar();
				this.CurrentToken = DryfileToken.OpenParen;
			}
			else if (this.currentChar == ')')
			{
				this.ConsumeChar();
				this.CurrentToken = DryfileToken.CloseParen;
			}
			else if (char.IsDigit((char)this.currentChar))
			{
				var foundPoint = false;
				this.stringBuilder.Clear();

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
					this.CurrentToken = DryfileToken.Float;
					this.CurrentFloat = Double.Parse(this.stringBuilder.ToString());
				}
				else
				{
					this.CurrentToken = DryfileToken.Integer;
					this.CurrentInteger = Int64.Parse(this.stringBuilder.ToString());
				}
			}
			else if ((char)this.currentChar == '@' || char.IsLetter((char)this.currentChar))
			{
				var isAnnotation = this.currentChar == '@';

				if (isAnnotation)
				{
					this.ConsumeChar();
				}

				this.stringBuilder.Clear();

				while (this.currentChar != -1 && (char.IsLetterOrDigit((char)this.currentChar) || (char)this.currentChar == '-'))
				{
					this.stringBuilder.Append((char)this.currentChar);

					this.ConsumeChar();
				}

				if (this.currentChar == '?')
				{
					this.stringBuilder.Append("?");
					this.ConsumeChar();
				}

				this.CurrentString = this.stringBuilder.ToString();

				DryfileKeyword keyword;

				if (isAnnotation)
				{
					this.CurrentToken = DryfileToken.Annotation;
				}
				else if (this.keywordsByName.TryGetValue(this.CurrentString, out keyword))
				{
					this.CurrentKeyword = keyword;
					this.CurrentToken = DryfileToken.Keyword;
				}
				else
				{
					this.CurrentToken = DryfileToken.Identifier;
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
