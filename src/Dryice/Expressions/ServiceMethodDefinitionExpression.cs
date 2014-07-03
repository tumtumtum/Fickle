using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Dryice.Model;

namespace Dryice.Expressions
{
	public class ServiceMethodDefinitionExpression
		: MethodDefinitionExpression
	{
		public ServiceMethod ServiceMethod
		{
			get;
			private set;
		}

		public ServiceMethodDefinitionExpression(string name, ReadOnlyCollection<Expression> parameters, Type returnType, Expression body, bool isPredeclaration, string rawAttributes, ServiceMethod serviceMethod)
			: base(name, parameters, returnType, body, isPredeclaration, rawAttributes)
		{
			this.ServiceMethod = serviceMethod;
		}
	}
}
