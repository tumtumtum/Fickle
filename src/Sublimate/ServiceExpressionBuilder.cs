//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//


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

		public virtual Expression Build(ServiceTypeProperty property)
		{
			return new PropertyDefinitionExpression(property.Name, this.ServiceModel.GetServiceType(property.TypeName));
		}

		public virtual Expression Build(ServiceType serviceType)
		{
			var propertyDefinitions = serviceType.Properties.Select(Build).ToList();

			return new TypeDefinitionExpression(null, new CodeBlockExpression(new ReadOnlyCollection<Expression>(propertyDefinitions)), serviceType.Name, this.ServiceModel.GetServiceType("ServiceObject"));
		}

		public virtual Expression Build(ServiceParameter parameter, int index)
		{
			return new ParameterDefinitionExpression(parameter.Name, this.ServiceModel.GetServiceType(parameter.TypeName), index);
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

			return new TypeDefinitionExpression(null, new CodeBlockExpression(new ReadOnlyCollection<Expression>(methodDefinitions)), serviceGateway.Name, this.ServiceModel.GetServiceType("ServiceGateway"));
		}
	}
}
