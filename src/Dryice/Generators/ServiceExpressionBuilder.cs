//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators
{
	public class ServiceExpressionBuilder
	{
		public ServiceModel ServiceModel { get; private set; }
		public CodeGenerationOptions CodeGenerationOptions { get; private set; }

		public ServiceExpressionBuilder(ServiceModel serviceModel, CodeGenerationOptions codeGenerationOptions)
		{
			this.ServiceModel = serviceModel;
			this.CodeGenerationOptions = codeGenerationOptions;
		}

		public virtual Type GetTypeFromName(string name)
		{
			return this.ServiceModel.GetTypeFromName(name);
		}

		public virtual Expression Build(ServiceProperty property)
		{
			return new PropertyDefinitionExpression(property.Name, this.GetTypeFromName(property.TypeName));
		}

		public virtual Expression Build(ServiceClass serviceClass)
		{
			Type baseType = null;
			var propertyDefinitions = serviceClass.Properties.Select(this.Build).ToList();

			if (!string.IsNullOrEmpty(this.CodeGenerationOptions.BaseTypeTypeName))
			{
				baseType = new DryType(this.CodeGenerationOptions.BaseTypeTypeName);
			}

			if (baseType == null && !string.IsNullOrEmpty(serviceClass.BaseTypeName))
			{
				baseType = new DryType(serviceClass.BaseTypeName);
			}

			if (baseType == null)
			{
				baseType = typeof(object);
			}

			return new TypeDefinitionExpression(this.GetTypeFromName(serviceClass.Name), baseType, null, propertyDefinitions.ToGroupedExpression(), false);
		}

		public virtual Expression Build(ServiceParameter parameter)
		{
			return Expression.Parameter(this.GetTypeFromName(parameter.TypeName), parameter.Name);
		}

		public virtual Expression Build(ServiceMethod method)
		{
			var parameterExpressions = new ReadOnlyCollection<Expression>(method.Parameters.Select(this.Build).ToList());

			var attributes = new Dictionary<string, string>();

			method.GetType().GetProperties().ForEach(c => attributes[c.Name] = Convert.ToString(c.GetValue(method)));

			return new MethodDefinitionExpression(method.Name, parameterExpressions, this.ServiceModel.GetTypeFromName(method.Returns), null, true, null, new ReadOnlyDictionary<string, string>(attributes));
		}

		public virtual Expression Build(ServiceGateway serviceGateway)
		{
			Type baseType = null;
			
			if (!string.IsNullOrEmpty(this.CodeGenerationOptions.BaseTypeTypeName))
			{
				baseType = new DryType(this.CodeGenerationOptions.BaseTypeTypeName);
			}

			if (baseType == null && !string.IsNullOrEmpty(serviceGateway.BaseTypeName))
			{
				baseType = new DryType(serviceGateway.BaseTypeName);
			}

			if (baseType == null)
			{
				baseType = typeof(object);
			}

			var methodDefinitions = serviceGateway.Methods.Select(this.Build).ToList();

			var attributes = new Dictionary<string, string>()
			{
				{ "Hostname", serviceGateway.Hostname }
			};

			return new TypeDefinitionExpression(new DryType(serviceGateway.Name), baseType, null, methodDefinitions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide), false, new ReadOnlyDictionary<string, string>(attributes), null);
		}
	}
}
