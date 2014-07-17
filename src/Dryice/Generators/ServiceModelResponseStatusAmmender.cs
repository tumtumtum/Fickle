using System;
using System.Collections.Generic;
using System.Linq;
using Dryice.Model;
using Platform;

namespace Dryice.Generators
{
	public class ServiceModelResponseStatusAmmender
	{
		private readonly ServiceModel serviceModel;
		protected readonly CodeGenerationOptions options;
		private readonly List<ServiceClass> additionalClasses = new List<ServiceClass>();

		public ServiceModelResponseStatusAmmender(ServiceModel serviceModel, CodeGenerationOptions options)
		{
			this.serviceModel = serviceModel;
			this.options = options;
		}

		protected virtual ServiceClass CreateValueResponseServiceClass(Type type)
		{
			var isNullable = DryNullable.GetUnderlyingType(type) != null;

			if (isNullable)
			{
				type = DryNullable.GetUnderlyingType(type);
			}

			var typeName = TypeSystem.GetPrimitiveName(type);
			var classPrefix = typeName;

			if (isNullable)
			{
				classPrefix = "Optional";
				typeName += "?";
			}

			var properties = new List<ServiceProperty>
			{
				new ServiceProperty()
				{
					Name = options.ResponseStatusPropertyName,
					TypeName = options.ResponseStatusTypeName
				},
				new ServiceProperty()
				{
					Name = "Value",
					TypeName = typeName
				},
			};

			return new ServiceClass(classPrefix + "ValueResponse", null, properties);
		}

		private ServiceClass AmmendOrCreateResponseStatusClass()
		{
			var retval = this.serviceModel.GetServiceClass(options.ResponseStatusTypeName);
			var properties = new List<ServiceProperty>
			{
				new ServiceProperty
				{
					Name = "Message",
					TypeName = "string"
				},
				new ServiceProperty
				{
					Name = "ErrorCode",
					TypeName = "string"
				},
				new ServiceProperty
				{
					Name = "HttpStatus",
					TypeName = "int"
				}
			};

			if (retval == null)
			{	
				retval = new ServiceClass(this.options.ResponseStatusTypeName,  null, properties);
			}
			else
			{
				foreach (var property in properties.Where(property => !retval.Properties.Exists(c => String.Equals(c.Name, property.Name, StringComparison.InvariantCultureIgnoreCase))))
				{
					retval.Properties.Add(property);
				}
			}

			return retval;
		}

		public virtual ServiceModel Ammend()
		{
			var returnTypes = serviceModel.Gateways.SelectMany(c => c.Methods).Select(c => serviceModel.GetTypeFromName(c.Returns)).ToHashSet();
			var returnServiceClasses = returnTypes.Where(TypeSystem.IsNotPrimitiveType).Select(serviceModel.GetServiceClass);

			var containsResponseStatus = serviceModel.GetServiceClass(options.ResponseStatusTypeName) != null;

			var responseStatusClass = this.AmmendOrCreateResponseStatusClass();

			if (!containsResponseStatus)
			{
				additionalClasses.Add(responseStatusClass);
			} 
			
			foreach (var type in returnTypes.Where(TypeSystem.IsPrimitiveType))
			{ 
				var valueResponse = CreateValueResponseServiceClass(type);

				additionalClasses.Add(valueResponse);
			}

			foreach (var returnTypeClass in returnServiceClasses)
			{
				if (!returnTypeClass.Properties.Exists(c => string.Equals(c.Name, options.ResponseStatusPropertyName, StringComparison.CurrentCultureIgnoreCase)))
				{
					returnTypeClass.Properties.Add(new ServiceProperty
					{
						Name = options.ResponseStatusPropertyName,
						TypeName = options.ResponseStatusTypeName
					});
				}
			}

			return new ServiceModel(serviceModel.Enums, serviceModel.Classes.Concat(additionalClasses), serviceModel.Gateways);
		}
	}
}
