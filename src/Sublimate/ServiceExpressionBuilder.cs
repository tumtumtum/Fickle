//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Sublimate.Expressions;
using Sublimate.Model;

namespace Sublimate
{
	public class ServiceExpressionBuilder
	{
		public ServiceModel ServiceModel { get; private set; }
		
		public ServiceExpressionBuilder(ServiceModel serviceModel)
		{
			this.ServiceModel = serviceModel;
		}

		public virtual Type GetTypeFromName(string name)
		{
			var type = TypeSystem.GetPrimitiveType(name);

			if (type != null)
			{
				return type;
			}

			return this.ServiceModel.GetServiceType(name);
		}

		public virtual Expression Build(ServiceProperty property)
		{
			return new PropertyDefinitionExpression(property.Name, this.GetTypeFromName(property.TypeName));
		}

		public virtual Expression Build(ServiceClass serviceClass)
		{
			var propertyDefinitions = serviceClass.Properties.Select(Build).ToList();

			return new TypeDefinitionExpression(this.GetTypeFromName(serviceClass.Name), new SublimateType("ServiceObject"), null, new GroupedExpressionsExpression(new ReadOnlyCollection<Expression>(propertyDefinitions)));
		}

		public virtual Expression Build(ServiceParameter parameter, int index)
		{
			return new ParameterDefinitionExpression(parameter.Name, this.GetTypeFromName(parameter.TypeName), index);
		}

		public virtual Expression Build(ServiceMethod method)
		{
			var i = 0;
			var parameterExpressions = new ReadOnlyCollection<Expression>(method.Parameters.Select(c => Build(c, i++)).ToList());

			return new MethodDefinitionExpression(method.Name, parameterExpressions, this.ServiceModel.GetServiceType(method.ReturnTypeName));
		}

		public virtual Expression Build(ServiceGateway serviceGateway)
		{
			var methodDefinitions = serviceGateway.Methods.Select(Build).ToList();

			return new TypeDefinitionExpression(new SublimateType(serviceGateway.Name), new SublimateType("ServiceGateway"), null, new GroupedExpressionsExpression(methodDefinitions), false, null);
		}
	}
}
