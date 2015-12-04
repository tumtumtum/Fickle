using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fickle.Expressions.Fluent
{
	public class TypeDefinitionExpressionScope<T>
		: IExpressionScope<T>
		where T : class
	{
		private readonly Type type;
		protected readonly T previousScope;
		private readonly Action<Expression> complete;
		private readonly List<Expression> headerExpressions = new List<Expression>();
		private readonly List<Expression> bodyExpressions = new List<Expression>();
		private bool isPredeclaration;
		private readonly List<Type> interfaceTypes = new List<Type>();
		private readonly Dictionary<string, string> attributes = new Dictionary<string, string>();

		public TypeDefinitionExpressionScope(Type type, T previousScope, Action<Expression> complete)
		{
			this.type = type;
			this.previousScope = previousScope;
			this.complete = complete;
		}

		public TypeDefinitionExpressionScope<T> AddHeader(Expression header)
		{
			this.headerExpressions.Add(header);

			return this;
		}

		public TypeDefinitionExpressionScope<T> SetIsPredeclaration(bool value)
		{
			this.isPredeclaration = value;

			return this;
		}

		public TypeDefinitionExpressionScope<T> Implement(params Type[] interfaces)
		{
			this.interfaceTypes.AddRange(interfaces);

			return this;
		}

		public TypeDefinitionExpressionScope<T> SetAttribute(string name, string value)
		{
			this.attributes[name] = value;

			return this;
		}

		public MethodDefinitionScope<TypeDefinitionExpressionScope<T>> AddMethod(string name, Type returnType, object parameters)
		{
			return new MethodDefinitionScope<TypeDefinitionExpressionScope<T>>(name, returnType, parameters, this, c => this.bodyExpressions.Add(c));
		}

		public virtual T End()
		{
			this.complete(new TypeDefinitionExpression(this.type, this.headerExpressions.ToGroupedExpression(), this.bodyExpressions.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide), this.isPredeclaration, new ReadOnlyDictionary<string, string>(this.attributes), new ReadOnlyCollection<Type>(this.interfaceTypes)));

			return this.previousScope;
		}
	}
}