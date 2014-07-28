using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dryice.Model;
using Platform.Reflection;

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
			if (this.tokenizer.CurrentToken == DryfileToken.Keyword)
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
				throw new UnexpectedDryfileTokenException(this.tokenizer.CurrentToken, this.tokenizer.CurrentValue, DryfileToken.Keyword);
			}
		}

		protected virtual void Expect(params DryfileToken[] tokens)
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

			if (this.tokenizer.CurrentToken == DryfileToken.Colon)
			{
				this.ReadNextToken();

				this.Expect(DryfileToken.Integer);

				retval.Value = (int)this.tokenizer.CurrentInteger;

				this.ReadNextToken();
			}

			return retval;
		}
			
		protected virtual ServiceEnum ProcessEnum()
		{
			this.ReadNextToken();

			this.Expect(DryfileToken.Identifier);

			var retval = new ServiceEnum
			{
				Name = this.tokenizer.CurrentIdentifier,
				Values = new List<ServiceEnumValue>()
			};

			this.ReadNextToken();
			this.Expect(DryfileToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken != DryfileToken.Identifier)
				{
					break;
				}

				var enumValue = this.ProcessEnumValue();

				retval.Values.Add(enumValue);
			}

			this.Expect(DryfileToken.Dedent);
			this.ReadNextToken();

			return retval;
		}

		protected virtual string ParseTypeName()
		{
			var builder = new StringBuilder();

			if (this.tokenizer.CurrentToken == DryfileToken.OpenBracket)
			{
				builder.Append('[');

				this.ReadNextToken();
				builder.Append(this.ParseTypeName());

				this.Expect(DryfileToken.CloseBracket);
				builder.Append(']');
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == DryfileToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}
			}
			else
			{
				this.Expect(DryfileToken.Identifier);

				builder.Append(this.tokenizer.CurrentIdentifier);
				this.ReadNextToken();

				if (this.tokenizer.CurrentToken == DryfileToken.QuestionMark)
				{
					builder.Append('?');
					this.ReadNextToken();
				}

				if (this.tokenizer.CurrentToken == DryfileToken.OpenBracket)
				{	
					this.ReadNextToken();
					this.Expect(DryfileToken.CloseBracket);
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

			this.Expect(DryfileToken.Colon);

			this.ReadNextToken();

			retval.TypeName = this.ParseTypeName();
			
			return retval;
		}

		protected virtual ServiceClass ProcessClass()
		{
			this.ReadNextToken();

			this.Expect(DryfileToken.Identifier);

			var retval = new ServiceClass(this.tokenizer.CurrentIdentifier, null, new List<ServiceProperty>());

			this.ReadNextToken();
			this.Expect(DryfileToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == DryfileToken.Identifier)
				{
					var property = this.ProcessProperty();

					retval.Properties.Add(property);
				}
				else if (this.tokenizer.CurrentToken == DryfileToken.Annotation)
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

			this.Expect(DryfileToken.Dedent);
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
			this.Expect(DryfileToken.Colon);

			this.ReadNextToken();
			this.Expect(DryfileToken.Identifier);

			retval.TypeName = this.tokenizer.CurrentIdentifier;

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == DryfileToken.QuestionMark)
			{
				retval.TypeName += "?";

				this.ReadNextToken();
			}
			
			if (this.tokenizer.CurrentToken == DryfileToken.OpenBracket)
			{
				this.ReadNextToken();
				this.Expect(DryfileToken.CloseBracket);
				this.ReadNextToken();
				retval.TypeName += "[]";
			}

			return retval;
		}

		protected virtual ServiceMethod ProcessMethod()
		{
			var retval = new ServiceMethod
			{
				Name = this.tokenizer.CurrentIdentifier
			};

			this.ReadNextToken();

			if (this.tokenizer.CurrentToken == DryfileToken.OpenParen)
			{
				this.ReadNextToken();

				var parameters = new List<ServiceParameter>();

				while (this.tokenizer.CurrentToken != DryfileToken.CloseParen && this.tokenizer.CurrentToken != DryfileToken.EndOfFile)
				{
					parameters.Add(this.ProcessParameter());
				}

				retval.Parameters = parameters;

				this.ReadNextToken();
			}

			if (this.tokenizer.CurrentToken == DryfileToken.Indent)
			{
				this.ReadNextToken();

				while (this.tokenizer.CurrentToken != DryfileToken.Dedent
					&& this.tokenizer.CurrentToken != DryfileToken.EndOfFile)
				{
					if (this.tokenizer.CurrentToken == DryfileToken.Annotation)
					{
						var annotation = this.ProcessAnnotation();

						this.SetAnnotation(retval, annotation);

						if (annotation.Key == "content")
						{
							var contentParameterName = annotation.Value.Trim();

							var serviceParameter = retval.Parameters.FirstOrDefault(c => c.Name == contentParameterName);

							retval.ContentServiceParameter = serviceParameter;
						}
					}
					else
					{
						throw new UnexpectedDryfileTokenException(this.tokenizer.CurrentToken, null, DryfileToken.Annotation);
					}
				}

				this.Expect(DryfileToken.Dedent);

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
				if (property.GetFirstCustomAttribute<ServiceAnnotationAttribute>(true) == null)
				{
					return false;
				}

				property.SetValue(target, Convert.ChangeType(annotation.Value.Trim(), property.PropertyType), null);

				return true;
			}

			return false;
		}

		protected virtual ServiceGateway ProcessGateway()
		{
			this.ReadNextToken();

			this.Expect(DryfileToken.Identifier);

			var retval = new ServiceGateway
			{
				Name = this.tokenizer.CurrentIdentifier,
				Methods = new List<ServiceMethod>()
			};

			this.ReadNextToken();
			this.Expect(DryfileToken.Indent);
			this.ReadNextToken();

			while (true)
			{
				if (this.tokenizer.CurrentToken == DryfileToken.Identifier)
				{
					var method = this.ProcessMethod();

					retval.Methods.Add(method);
				}
				else if (this.tokenizer.CurrentToken == DryfileToken.Annotation)
				{
					var annotation = this.ProcessAnnotation();

					this.SetAnnotation(retval, annotation);
				}
				else
				{
					break;
				}
			}

			this.Expect(DryfileToken.Dedent);
			this.ReadNextToken();

			return retval;
		}

		protected virtual ServiceModel Parse()
		{
			while (this.tokenizer.CurrentToken != DryfileToken.EndOfFile)
			{
				this.ProcessTopLevel();
			}

			return new ServiceModel(enums, classes, gateways);
		}
	}
}
