using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dryice.Model;

namespace Dryice.Dryfile
{
	public class DryfileParser
	{
		private readonly DryfileTokenizer tokenizer;
		private readonly List<ServiceEnum> enums = new List<ServiceEnum>();
		private readonly List<ServiceClass> classes = new List<ServiceClass>();
		private readonly List<ServiceGateway> gateways = new List<ServiceGateway>();
	
		public DryfileParser(TextReader reader)
		{
			this.tokenizer = new DryfileTokenizer(reader);

			this.tokenizer.ReadNextToken();
		}

		public static ServiceModel Parse(string s)
		{
			return Parse(new StringReader(s));
		}

		public static ServiceModel Parse(TextReader reader)
		{
			var parser = new DryfileParser(reader);

			return parser.Parse();
		}

		protected virtual void ProcessTopLevel()
		{
			if (this.tokenizer.CurrentToken == DryfilelToken.Keyword)
			{
				switch (this.tokenizer.CurrentKeyword)
				{
					case DryfileKeyword.Class:
						this.classes.Add(this.ProcessClass());
						break;
					case DryfileKeyword.Enum:
						this.enums.Add(this.ProcessEnum());
						break;
					case DryfileKeyword.Gateway:
						this.gateways.Add(this.ProcessGateway());
						break;
				}
			}
			else
			{
				throw new UnexpectedDryfileTokenException(this.tokenizer.CurrentToken, this.tokenizer.CurrentValue, DryfilelToken.Keyword);
			}
		}

		protected virtual void Expect(params DryfilelToken[] tokens)
		{
			if (!tokens.Contains(this.tokenizer.CurrentToken))
			{
				throw new UnexpectedDryfileTokenException(this.tokenizer.CurrentToken, this.tokenizer.CurrentValue, tokens);
			}
		}

		protected virtual ServiceEnumValue ProcessEnumValue()
		{
			var retval = new ServiceEnumValue
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == DryfilelToken.Colon)
			{
				this.ReadNextToken();

				this.Expect(DryfilelToken.Integer);

				retval.Value = (int)this.tokenizer.CurrentInteger;

				this.ReadNextToken();
			}

			return retval;
		}
			
		protected virtual ServiceEnum ProcessEnum()
		{
			this.ReadNextToken();

			this.Expect(DryfilelToken.Identifier);

			var retval = new ServiceEnum
			{
				Name = this.tokenizer.CurrentIdentifier,
				Values = new List<ServiceEnumValue>()
			};

			this.ReadNextToken();
			this.Expect(DryfilelToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken != DryfilelToken.Identifier)
				{
					break;
				}

				var enumValue = this.ProcessEnumValue();

				retval.Values.Add(enumValue);
			}

			this.Expect(DryfilelToken.Dedent);
			this.ReadNextToken();

			return retval;
		}

		protected virtual string ParseTypeName()
		{
			var builder = new StringBuilder();

			if (this.tokenizer.CurrentToken == DryfilelToken.OpenBracket)
			{
				builder.Append('[');

				this.ReadNextToken();
				builder.Append(this.ParseTypeName());

				this.Expect(DryfilelToken.CloseBracket);
				builder.Append(']');
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == DryfilelToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}
			}
			else
			{
				this.Expect(DryfilelToken.Identifier);

				builder.Append(this.tokenizer.CurrentIdentifier);
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == DryfilelToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}

				if (this.tokenizer.CurrentToken == DryfilelToken.OpenBracket)
				{	
					this.ReadNextToken();
					this.Expect(DryfilelToken.CloseBracket);
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

			this.Expect(DryfilelToken.Colon);

			this.ReadNextToken();

			retval.TypeName = this.ParseTypeName();
			
			return retval;
		}

		protected virtual ServiceClass ProcessClass()
		{
			this.ReadNextToken();

			this.Expect(DryfilelToken.Identifier);

			var retval = new ServiceClass
			{
				Name = this.tokenizer.CurrentIdentifier,
				Properties = new List<ServiceProperty>()
			};

			this.ReadNextToken();
			this.Expect(DryfilelToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == DryfilelToken.Identifier)
				{
					var property = this.ProcessProperty();

					retval.Properties.Add(property);
				}
				else if (this.tokenizer.CurrentToken == DryfilelToken.Annotation)
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

			this.Expect(DryfilelToken.Dedent);
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
			this.Expect(DryfilelToken.Colon);

			this.ReadNextToken();
			this.Expect(DryfilelToken.Identifier);

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

			this.Expect(DryfilelToken.OpenParen);

			this.ReadNextToken();

			var parameters = new List<ServiceParameter>();

			while (this.tokenizer.CurrentToken != DryfilelToken.CloseParen && this.tokenizer.CurrentToken != DryfilelToken.EndOfFile)
			{
				parameters.Add(this.ProcessParameter());
			}

			retval.Parameters = parameters;

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == DryfilelToken.Indent)
			{
				this.ReadNextToken();

				while (this.tokenizer.CurrentToken != DryfilelToken.Dedent
					&& this.tokenizer.CurrentToken != DryfilelToken.EndOfFile)
				{
					if (this.tokenizer.CurrentToken == DryfilelToken.Annotation)
					{
						var annotation = this.ProcessAnnotation();

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

				this.Expect(DryfilelToken.Dedent);

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

			this.Expect(DryfilelToken.Identifier);

			var retval = new ServiceGateway
			{
				Name = this.tokenizer.CurrentIdentifier,
				Methods = new List<ServiceMethod>()
			};

			this.ReadNextToken();
			this.Expect(DryfilelToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == DryfilelToken.Identifier)
				{
					var method = this.ProcessMethod();

					retval.Methods.Add(method);
				}
				else if (this.tokenizer.CurrentToken == DryfilelToken.Annotation)
				{
					var annotation = this.ProcessAnnotation();

					this.SetAnnotation(retval, annotation);
				}
				else
				{
					break;
				}
			}

			this.Expect(DryfilelToken.Dedent);
			this.ReadNextToken();

			return retval;
		}

		protected virtual ServiceModel Parse()
		{
			while (this.tokenizer.CurrentToken != DryfilelToken.EndOfFile)
			{
				this.ProcessTopLevel();
			}

			return new ServiceModel(enums, classes, gateways);
		}
	}
}
