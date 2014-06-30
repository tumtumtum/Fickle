using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dryice.Webs
{
	public class WebsTokenizer
	{
		private int currentChar;
		private int workingIndent;
		private bool encounteredSymbolOnCurrentLine;
		private readonly StringBuilder stringBuilder;
		private readonly Stack<int> indentStack; 
		public TextReader Reader { get; set; }
		public WebsKeyword CurrentKeyword { get; private set; }
		public WebsToken CurrentToken { get; private set; }
		public string CurrentString { get; private set; }
		public long CurrentInteger { get; private set; }
		public double CurrentFloat { get; private set; }
		private readonly Dictionary<string, WebsKeyword> keywordsByName = new Dictionary<string, WebsKeyword>(StringComparer.InvariantCultureIgnoreCase);
		
		public WebsTokenizer(TextReader reader)
		{
			currentChar = -1;
			this.Reader = reader;
			this.CurrentToken = WebsToken.None;
			this.stringBuilder = new StringBuilder();

			foreach (var name in Enum.GetNames(typeof(WebsKeyword)))
			{
				keywordsByName[name] = (WebsKeyword)Enum.Parse(typeof(WebsKeyword), name);
			}

			this.indentStack = new Stack<int>();

			this.indentStack.Push(0);

			this.ConsumeChar();
		}

		private int CurrentIndent
		{
			get
			{
				return indentStack.Peek();
			}
			set
			{
				indentStack.Pop();
				indentStack.Push(value);
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
					case WebsToken.Colon:
						return ':';
					case WebsToken.EndOfFile:
						return null;
					case WebsToken.Float:
						return this.CurrentFloat;
					case WebsToken.Integer:
						return this.CurrentInteger;
					case WebsToken.Keyword:
						return this.CurrentKeyword;
					case WebsToken.None:
						return null;
					case WebsToken.StringLiteral:
						return this.CurrentString;
					case WebsToken.Identifier:
						return this.CurrentString;
					default:
						return null;
				}	
			}
		}

		private void ConsumeChar()
		{
			currentChar = this.Reader.Read();
		}

		public void ReadStringToEnd()
		{
			var nonWhitespaceEncountered = false;
			var builder = new StringBuilder();

			while (true)
			{
				if (currentChar == -1)
				{
					break;
				}
				else if (currentChar == '\n')
				{
					this.ConsumeChar();

					break;
				}
				else if (currentChar == '\r')
				{
					this.ConsumeChar();

					continue;
				}

				if (currentChar == ' ' || currentChar == '\t' && !nonWhitespaceEncountered)
				{
					this.ConsumeChar();

					continue;
				}

				nonWhitespaceEncountered = true;

				builder.Append((char)currentChar);

				this.ConsumeChar();
			}

			this.CurrentToken = WebsToken.StringLiteral;
			this.CurrentString = builder.ToString();
		}

		public WebsToken ReadNextToken()
		{
			if (this.CurrentToken == WebsToken.Dedent && workingIndent > 0)
			{
				if (workingIndent != this.CurrentIndent)
				{
					indentStack.Pop();

					return this.CurrentToken;
				}
				else
				{
					workingIndent = 0;
				}
			}
			

			while (char.IsWhiteSpace((char)currentChar))
			{
				if (currentChar == '\n')
				{
					encounteredSymbolOnCurrentLine = false;
					workingIndent = 0;
				}
				else if (currentChar == ' ' || currentChar == '\t')
				{
					if (!encounteredSymbolOnCurrentLine)
					{
						workingIndent += currentChar == '\t' ? 4 : 1;
					}
				}

				this.ConsumeChar();
			}

			if (currentChar == -1)
			{
				if (this.indentStack.Count > 1)
				{
					this.indentStack.Pop();
					workingIndent = this.indentStack.Peek();

					this.CurrentToken = WebsToken.Dedent;

					return this.CurrentToken;
				}
				else
				{
					this.CurrentToken = WebsToken.EndOfFile;

					return this.CurrentToken;
				}
			}

			if (!encounteredSymbolOnCurrentLine)
			{
				encounteredSymbolOnCurrentLine = true;

				if (this.CurrentIndent != workingIndent)
				{
					if (workingIndent > this.CurrentIndent)
					{
						this.indentStack.Push(workingIndent);

						this.CurrentToken = WebsToken.Indent;

						return this.CurrentToken;
					}
					else
					{
						this.indentStack.Pop();

						this.CurrentToken = WebsToken.Dedent;

						return this.CurrentToken;
					}
				}
			}

			if (currentChar == ':')
			{
				this.ConsumeChar();
				this.CurrentToken = WebsToken.Colon;
			}
			else if (currentChar == '?')
			{
				this.ConsumeChar();
				this.CurrentToken = WebsToken.QuestionMark;
			}
			else if (currentChar == '[')
			{
				this.ConsumeChar(); 
				this.CurrentToken = WebsToken.OpenBracket;
			}
			else if (currentChar == ']')
			{
				this.ConsumeChar(); 
				this.CurrentToken = WebsToken.CloseBracket;
			}
			else if (currentChar == '(')
			{
				this.ConsumeChar();
				this.CurrentToken = WebsToken.OpenParen;
			}
			else if (currentChar == ')')
			{
				this.ConsumeChar();
				this.CurrentToken = WebsToken.CloseParen;
			}
			else if (char.IsDigit((char)currentChar))
			{
				var foundPoint = false;
				stringBuilder.Clear();

				while (currentChar != -1 && (char.IsDigit((char)currentChar) || (currentChar == '.' && !foundPoint)))
				{
					if (currentChar == '.')
					{
						foundPoint = true;
					}

					stringBuilder.Append((char)currentChar);

					this.ConsumeChar();
				}

				if (foundPoint)
				{
					this.CurrentToken = WebsToken.Float;
					this.CurrentFloat = Double.Parse(stringBuilder.ToString());
				}
				else
				{
					this.CurrentToken = WebsToken.Integer;
					this.CurrentInteger = Int64.Parse(stringBuilder.ToString());
				}
			}
			else if ((char)currentChar == '@' || char.IsLetter((char)currentChar))
			{
				var isAnnotation = currentChar == '@';

				if (isAnnotation)
				{
					this.ConsumeChar();
				}

				stringBuilder.Clear();

				while (currentChar != -1 && (char.IsLetterOrDigit((char)currentChar) || (char)currentChar == '-'))
				{
					stringBuilder.Append((char)currentChar);

					this.ConsumeChar();
				}

				this.CurrentString = stringBuilder.ToString();

				WebsKeyword keyword;

				if (isAnnotation)
				{
					this.CurrentToken = WebsToken.Annotation;
				}
				else if (keywordsByName.TryGetValue(this.CurrentString, out keyword))
				{
					this.CurrentKeyword = keyword;
					this.CurrentToken = WebsToken.Keyword;
				}
				else
				{
					this.CurrentToken = WebsToken.Identifier;
				}
			}
			else
			{
				throw new InvalidOperationException("Unexpected char: '" + (char)currentChar + "'");
			}

			return this.CurrentToken;
		}
	}
}
