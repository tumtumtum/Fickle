//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Model;

namespace Dryice
{
	public class ReferencedTypesCollector
		: ServiceExpressionVisitor
	{
		private readonly HashSet<Type> referencedTypes = new HashSet<Type>();

		private ReferencedTypesCollector()
		{	
		}

		public static List<Type> CollectReferencedTypes(Expression expression)
		{
			var collector = new ReferencedTypesCollector();

			collector.Visit(expression);

			return collector.referencedTypes.ToList();
		}

		protected override Expression VisitPropertyDefinitionExpression(Expressions.PropertyDefinitionExpression property)
		{
			referencedTypes.Add(property.PropertyType);

			return base.VisitPropertyDefinitionExpression(property);
		}

		protected override Expression VisitParameterDefinitionExpression(Expressions.ParameterDefinitionExpression parameter)
		{
			referencedTypes.Add(parameter.ParameterType);

			return base.VisitParameterDefinitionExpression(parameter);
		}

		protected override Expression VisitMethodDefinitionExpression(Expressions.MethodDefinitionExpression method)
		{
			referencedTypes.Add(method.ReturnType);

			return base.VisitMethodDefinitionExpression(method);
		}
	}
}
