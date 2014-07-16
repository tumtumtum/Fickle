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

		public bool IsCStyleFunction{ get; private set; }
		public bool IsStatic { get; private set; }
		public bool IsPredeclatation { get; private set; }
		public string Name { get; private set; }
		public string RawAttributes { get; private set; }
		public Type ReturnType { get; private set; }
		public ReadOnlyCollection<Expression> Parameters { get; private set; }
		public Expression Body { get; private set; }
		public ReadOnlyDictionary<string, string> Attributes { get; private set; }

		public MethodDefinitionExpression(string name, ReadOnlyCollection<Expression> parameters, Type returnType, Expression body, bool isPredeclaration, string rawAttributes = "", ReadOnlyDictionary<string, string> attributes = null, bool isStatic = false, bool isCStyleFunction = false)
		{
			this.RawAttributes = rawAttributes;
			this.IsStatic = isStatic;
			this.Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
			this.Name = name;
			this.ReturnType = returnType;
			this.Parameters = parameters;
			this.Body = body;
			this.IsPredeclatation = isPredeclaration;
			this.IsCStyleFunction = isCStyleFunction;
		}
	}
}
