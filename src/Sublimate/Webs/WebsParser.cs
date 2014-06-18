using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == WebsToken.Colon)
			{
				this.ReadNextToken();

				this.Expect(WebsToken.Integer);

				retval.Value = (int)this.tokenizer.CurrentInteger;

				this.ReadNextToken();
			}

			return retval;
		}
			
		protected virtual ServiceEnum ProcessEnum()
		{
			this.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceEnum
			{
				Name = this.tokenizer.CurrentIdentifier,
				Values = new List<ServiceEnumValue>()
			};

			this.ReadNextToken();
			this.Expect(WebsToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken != WebsToken.Identifier)
				{
					break;
				}

				var enumValue = ProcessEnumValue();

				retval.Values.Add(enumValue);
			}

			this.Expect(WebsToken.Dedent);
			this.ReadNextToken();

			return retval;
		}

		protected virtual string ParseTypeName()
		{
			var builder = new StringBuilder();

			if (this.tokenizer.CurrentToken == WebsToken.OpenBracket)
			{
				builder.Append('[');

				this.ReadNextToken();
				builder.Append(this.ParseTypeName());

				this.Expect(WebsToken.CloseBracket);
				builder.Append(']');
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == WebsToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}
			}
			else
			{
				this.Expect(WebsToken.Identifier);

				builder.Append(this.tokenizer.CurrentIdentifier);
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == WebsToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}

				if (this.tokenizer.CurrentToken == WebsToken.OpenBracket)
				{	
					this.ReadNextToken();
					this.Expect(WebsToken.CloseBracket);
					this.ReadNextToken();
					builder.Append("[]");
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

			this.ReadNextToken();

			this.Expect(WebsToken.Colon);

			this.ReadNextToken();

			retval.TypeName = this.ParseTypeName();
			
			return retval;
		}

		protected virtual ServiceClass ProcessClass()
		{
			this.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceClass
			{
				Name = this.tokenizer.CurrentIdentifier,
				Properties = new List<ServiceProperty>()
			};

			this.ReadNextToken();
			this.Expect(WebsToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == WebsToken.Identifier)
				{
					var property = this.ProcessProperty();

					retval.Properties.Add(property);
				}
				else if (this.tokenizer.CurrentToken == WebsToken.Annotation)
				{
					var annotation = this.ProcessAnnotation();

					switch(annotation.Key)
					{
					case "extends":
						retval.BaseTypeName = annotation.Value;
						break;
					default:
						this.SetAnnotation(retval, annotation);
						break;
					}
				}
				else
				{
					break;
				}
			}

			this.Expect(WebsToken.Dedent);
			this.ReadNextToken();

			return retval;
		}
		
		private void ReadNextToken()
		{
			this.tokenizer.ReadNextToken();
		}

		protected virtual ServiceParameter ProcessParameter()
		{
			var retval = new ServiceParameter()
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.ReadNextToken();
			this.Expect(WebsToken.Colon);

			this.ReadNextToken();
			this.Expect(WebsToken.Identifier);

			retval.TypeName = this.tokenizer.CurrentIdentifier;

			this.ReadNextToken();

			return retval;
		}

		protected virtual ServiceMethod ProcessMethod()
		{
			var retval = new ServiceMethod
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.ReadNextToken();

			this.Expect(WebsToken.OpenParen);

			this.ReadNextToken();

			var parameters = new List<ServiceParameter>();

			while (this.tokenizer.CurrentToken != WebsToken.CloseParen && this.tokenizer.CurrentToken != WebsToken.EndOfFile)
			{
				parameters.Add(ProcessParameter());
			}

			retval.Parameters = parameters;

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == WebsToken.Indent)
			{
				this.ReadNextToken();

				while (this.tokenizer.CurrentToken != WebsToken.Dedent
					&& this.tokenizer.CurrentToken != WebsToken.EndOfFile)
				{
					if (this.tokenizer.CurrentToken == WebsToken.Annotation)
					{
						var annotation = ProcessAnnotation();

						if (annotation.Key == "content")
						{
							var contentTypeName = annotation.Value.Trim();

							var serviceParameter = retval.Parameters.FirstOrDefault(c => c.Name == contentTypeName);

							retval.Content = serviceParameter;
						}
						else
						{
							this.SetAnnotation(retval, annotation);
						}
					}
				}

				this.Expect(WebsToken.Dedent);

				this.ReadNextToken();
			}

			return retval;
		}

		private KeyValuePair<string, string> ProcessAnnotation()
		{
			var annotationName = this.tokenizer.CurrentString;

			this.tokenizer.ReadStringToEnd();

			var annotationValue = this.tokenizer.CurrentString.Trim();

			this.ReadNextToken();

			return new KeyValuePair<string, string>(annotationName, annotationValue);
		}

		private bool SetAnnotation(object target, KeyValuePair<string, string> annotation)
		{
			var property = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => c.Name.Equals(annotation.Key, StringComparison.InvariantCultureIgnoreCase));

			if (property != null)
			{
				property.SetValue(target, Convert.ChangeType(annotation.Value.Trim(), property.PropertyType), null);

				return true;
			}

			return false;
		}

		protected virtual ServiceGateway ProcessGateway()
		{
			this.ReadNextToken();

			this.Expect(WebsToken.Identifier);

			var retval = new ServiceGateway
			{
				Name = this.tokenizer.CurrentIdentifier,
				Methods = new List<ServiceMethod>()
			};

			this.ReadNextToken();
			this.Expect(WebsToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == WebsToken.Identifier)
				{
					var method = this.ProcessMethod();

					retval.Methods.Add(method);
				}
				else if (this.tokenizer.CurrentToken == WebsToken.Annotation)
				{
					var annotation = ProcessAnnotation();

					this.SetAnnotation(retval, annotation);
				}
				else
				{
					break;
				}
			}

			this.Expect(WebsToken.Dedent);
			this.ReadNextToken();

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
