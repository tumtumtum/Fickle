//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators
{
	public class ReferencedTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Type> referencedTypes = new HashSet<Type>();

		private ReferencedTypesCollector()
		{	
		}

		private void AddType(Type type)
		{
			referencedTypes.Add(type);

			var delegateType = type as DryDelegateType;

			if (delegateType != null)
			{
				this.AddType(delegateType.ReturnType);
				delegateType.Parameters.Select(c => c.ParameterType).ForEach(this.AddType);
			}

			var listType = type as DryListType;

			if (listType != null)
			{
				this.AddType(listType.ListElementType);
			}
		}

		public static List<Type> CollectReferencedTypes(Expression expression)
		{
			var collector = new ReferencedTypesCollector();

			collector.Visit(expression);

			return collector.referencedTypes.ToList();
		}

		protected override Expression VisitPropertyDefinitionExpression(Expressions.PropertyDefinitionExpression property)
		{
			AddType(property.PropertyType);

			return base.VisitPropertyDefinitionExpression(property);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			AddType(node.Type);

			return node;
		}

		protected override Expression VisitMethodDefinitionExpression(Expressions.MethodDefinitionExpression method)
		{
			AddType(method.ReturnType);

			return base.VisitMethodDefinitionExpression(method);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			this.AddType(expression.BaseType);

			return base.VisitTypeDefinitionExpression(expression);
		}
	}
}
