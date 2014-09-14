//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators
{
	/// <summary>
	/// Collects and returns a distinct list of all types referenced by the expression tree
	/// </summary>
	public class ReferencedTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Type> referencedTypes = new HashSet<Type>();

		private ReferencedTypesCollector()
		{	
		}

		private void AddType(Type type)
		{
			if (type == null)
			{
				return;
			}

			var delegateType = type as FickleDelegateType;

			if (delegateType != null)
			{
				this.AddType(delegateType.ReturnType);
				delegateType.Parameters.Select(c => c.ParameterType).ForEach(this.AddType);

				return;
			}

			var nullableType = type as FickleNullable;

			if (nullableType != null)
			{
				this.AddType(nullableType.UnderlyingType);

				return;
			}
			
			var listType = type as FickleListType;

			if (listType != null)
			{
				this.AddType(listType.ListElementType);

				return;
			}

			referencedTypes.Add(type);
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

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			AddType(method.ReturnType);

			return base.VisitMethodDefinitionExpression(method);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			this.AddType(expression.Type.BaseType);

			return base.VisitTypeDefinitionExpression(expression);
		}
	}
}
