using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Dryice.Expressions
{
	public class MethodDefinitionExpression
		: BaseExpression
	{
		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)ServiceExpressionType.MethodDefinition;
			}
		}

		public override Type Type { get { return typeof(void); } }

		public AccessModifiers AccessModifiers { get; private set; }
		public string Name { get; private set; }
		public string RawAttributes { get; private set; }
		public bool IsPredeclaration { get; private set; }
		public Type ReturnType { get; private set; }
		public Expression Body { get; private set; }
		public ReadOnlyCollection<Expression> Parameters { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes { get; private set; }
		public ReadOnlyCollection<Exception> Exceptions { get; private set; }

		public MethodDefinitionExpression(string name, IEnumerable<Expression> parameters, AccessModifiers accessModifiers, Type returnType, Expression body, bool isPredeclaration, string rawAttributes = "", ReadOnlyDictionary<string, string> attributes = null, IEnumerable<Exception> exceptions = null)
			: this(name, parameters.ToReadOnlyCollection(), accessModifiers, returnType, body, isPredeclaration, rawAttributes, attributes, exceptions)
		{	
		}

		public MethodDefinitionExpression(string name, IEnumerable<Expression> parameters, Type returnType, Expression body, bool isPredeclaration, string rawAttributes = "", ReadOnlyDictionary<string, string> attributes = null)
			: this(name, parameters.ToReadOnlyCollection(), AccessModifiers.None, returnType, body, isPredeclaration, rawAttributes, attributes)
		{
		}

		public MethodDefinitionExpression(string name, ReadOnlyCollection<Expression> parameters, AccessModifiers accessModifiers, Type returnType, Expression body, bool isPredeclaration, string rawAttributes = "", ReadOnlyDictionary<string, string> attributes = null, IEnumerable<Exception> exceptions = null)
		{
			this.RawAttributes = rawAttributes;
			this.Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
			this.Name = name;
			this.AccessModifiers = accessModifiers;
			this.ReturnType = returnType;
			this.Parameters = parameters;
			this.Body = body;
			this.IsPredeclaration = isPredeclaration;
			this.Exceptions = exceptions == null ? null : exceptions.ToReadOnlyCollection();
		}
	}
}
