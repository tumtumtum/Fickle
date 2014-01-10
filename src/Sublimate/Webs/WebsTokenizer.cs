using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sublimate.Webs
{
	public class WebsTokenizer
	{
		private int currentChar;
		private readonly StringBuilder stringBuilder;
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

			this.ConsumeChar();
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

		public WebsToken ReadNextTokenOrToEndOfLine()
		{
			return default(WebsToken);
		}

		public WebsToken ReadNextToken()
		{
			while (char.IsWhiteSpace((char)currentChar))
			{
				this.ConsumeChar();	
			}

			if (currentChar == -1)
			{
				this.CurrentToken = WebsToken.EndOfFile;
			}
			else if (currentChar == ':')
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
			else if (char.IsLetter((char)currentChar))
			{
				stringBuilder.Clear();

				while (currentChar != -1 && char.IsLetterOrDigit((char)currentChar))
				{
					stringBuilder.Append((char)currentChar);

					this.ConsumeChar();
				}

				this.CurrentString = stringBuilder.ToString();

				WebsKeyword keyword;

				if (keywordsByName.TryGetValue(this.CurrentString, out keyword))
				{
					this.CurrentKeyword = keyword;
					this.CurrentToken = WebsToken.Keyword;
				}
				else
				{
					this.CurrentToken = WebsToken.Identifier;
				}
			}
			else if (currentChar == '(')
			{
				stringBuilder.Clear();

				while (currentChar != -1 && currentChar != ')')
				{
					stringBuilder.Append((char)currentChar);

					this.ConsumeChar();
				}

				if (currentChar != -1)
				{
					this.ConsumeChar();
				}

				this.CurrentToken = WebsToken.StringLiteral;
			}
			else
			{
				throw new InvalidOperationException("Unexpected char: '" + (char)currentChar + "'");
			}

			return this.CurrentToken;
		}
	}
}
