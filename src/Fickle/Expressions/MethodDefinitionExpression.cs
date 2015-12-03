using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fickle.Expressions
{
	public class MethodDefinitionExpression
		: BaseExpression
	{
		public override ExpressionType NodeType => (ExpressionType)ServiceExpressionType.MethodDefinition;

		public string Name { get; }
		public string RawAttributes { get; }
		public Type ReturnType { get; }
		public Expression Body { get; }
		public override Type Type => typeof(void);
		public bool IsPredeclaration { get; private set; }
		public ReadOnlyCollection<Expression> Parameters { get; }
		public AccessModifiers AccessModifiers { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes { get; }
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
			this.Exceptions = exceptions?.ToReadOnlyCollection();
		}

		public MethodDefinitionExpression ChangePredeclaration(bool value)
		{
			return new MethodDefinitionExpression(this.Name, this.Parameters, this.ReturnType, this.Body, value, this.RawAttributes, this.Attributes);
		}
	}
}
