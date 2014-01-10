using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sublimate.Model;

namespace Sublimate.Webs
{
	public class WebsParser
	{
		private readonly WebsTokenizer tokenizer;
		private readonly ServiceModel serviceModel;
	
		public WebsParser(TextReader reader)
		{
			tokenizer = new WebsTokenizer(reader);

			this.serviceModel = new ServiceModel
			{
				Enums = new List<ServiceEnum>(),
				Classes = new List<ServiceClass>(),
				Gateways = new List<ServiceGateway>()
			};

			tokenizer.ReadNextToken();
		}

		public static ServiceModel Parse(string s)
		{
			return Parse(new StringReader(s));
		}

		public static ServiceModel Parse(TextReader reader)
		{
			var parser = new WebsParser(reader);

			parser.Parse();

			return parser.serviceModel;
		}

		protected virtual void ProcessTopLevel()
		{
			if (tokenizer.CurrentToken == WebsToken.Keyword)
			{
				switch (tokenizer.CurrentKeyword)
				{
					case WebsKeyword.Class:
						serviceModel.Classes.Add(this.ProcessClass());
						break;
					case WebsKeyword.Enum:
						serviceModel.Enums.Add(this.ProcessEnum());
						break;
					case WebsKeyword.Gateway:
						serviceModel.Gateways.Add(this.ProcessGateway());
						break;
				}
			}
			else
			{
				throw new UnexpectedWebsTokenException(tokenizer.CurrentToken, tokenizer.CurrentValue, WebsToken.Keyword);
			}
		}

		protected virtual void Expect(params WebsToken[] tokens)
		{
			if (!tokens.Contains(this.tokenizer.CurrentToken))
			{
				throw new UnexpectedWebsTokenException(this.tokenizer.CurrentToken, this.tokenizer.CurrentValue, tokens);
			}
		}

		protected virtual ServiceEnumValue ProcessEnumValue()
		{
			var retval = new ServiceEnumValue
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.tokenizer.ReadNextToken();

			if (this.tokenizer.CurrentToken == WebsToken.Colon)
			{
				this.tokenizer.ReadNextToken();

				this.Expect(WebsToken.Integer);

				retval.Value = (int)this.tokenizer.CurrentInteger;

				this.tokenizer.ReadNextToken();
			}

			return retval;
		}
			
		protected virtual ServiceEnum ProcessEnum()
		{
			this.tokenizer.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceEnum
			{
				Name = this.tokenizer.CurrentIdentifier,
				Values = new List<ServiceEnumValue>()
			};

			this.tokenizer.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken != WebsToken.Identifier)
				{
					break;
				}

				var enumValue = ProcessEnumValue();

				retval.Values.Add(enumValue);
			}

			return retval;
		}

		protected virtual string ParseTypeName()
		{
			var builder = new StringBuilder();

			if (this.tokenizer.CurrentToken == WebsToken.OpenBracket)
			{
				builder.Append('[');

				this.tokenizer.ReadNextToken();
				builder.Append(this.ParseTypeName());

				this.Expect(WebsToken.CloseBracket);
				builder.Append(']');
				this.tokenizer.ReadNextToken();

				if (this.tokenizer.CurrentToken == WebsToken.QuestionMark)
				{
					builder.Append('?');
					this.tokenizer.ReadNextToken();
				}
			}
			else
			{
				this.Expect(WebsToken.Identifier);

				builder.Append(this.tokenizer.CurrentIdentifier);
				this.tokenizer.ReadNextToken();

				if (this.tokenizer.CurrentToken == WebsToken.QuestionMark)
				{
					builder.Append('?');
					this.tokenizer.ReadNextToken();
				}
			}

			return builder.ToString();
		}

		protected virtual ServiceProperty ProcessProperty()
		{
			var retval = new ServiceProperty
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.tokenizer.ReadNextToken();

			this.Expect(WebsToken.Colon);

			this.tokenizer.ReadNextToken();

			retval.TypeName = this.ParseTypeName();
			
			return retval;
		}

		protected virtual ServiceClass ProcessClass()
		{
			this.tokenizer.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceClass
			{
				Name = this.tokenizer.CurrentIdentifier,
				Properties = new List<ServiceProperty>()
			};

			this.tokenizer.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken != WebsToken.Identifier)
				{
					break;
				}

				var property = this.ProcessProperty();

				retval.Properties.Add(property);
			}

			return retval;
		}

		protected virtual ServiceMethod ProcessMethod()
		{
			var retval = new ServiceMethod
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.tokenizer.ReadNextToken();

			this.Expect(WebsToken.StringLiteral);

			this.tokenizer.ReadNextToken();

			if (this.tokenizer.CurrentToken == WebsToken.StringLiteral)
			{
				retval.ContentTypeName = this.tokenizer.CurrentString;

				this.tokenizer.ReadNextToken();
			}

			if (this.tokenizer.CurrentToken == WebsToken.Colon)
			{
				this.tokenizer.ReadNextToken();

				retval.ReturnTypeName = this.ParseTypeName();
			}

			return retval;
		}

		protected virtual ServiceGateway ProcessGateway()
		{
			this.tokenizer.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceGateway
			{
				Name = this.tokenizer.CurrentIdentifier,
				Methods = new List<ServiceMethod>()
			};

			this.tokenizer.ReadNextToken();

			if (this.tokenizer.CurrentToken == WebsToken.StringLiteral)
			{
				retval.Url = this.tokenizer.CurrentString;

				this.tokenizer.ReadNextToken();
			}

			while (true)
			{
				if (this.tokenizer.CurrentToken != WebsToken.Identifier)
				{
					break;
				}

				var method = this.ProcessMethod();

				retval.Methods.Add(method);
			}

			return retval;
		}

		protected virtual ServiceModel Parse()
		{
			while (tokenizer.CurrentToken != WebsToken.EndOfFile)
			{
				this.ProcessTopLevel();
			}

			return serviceModel;
		}
	}
}
