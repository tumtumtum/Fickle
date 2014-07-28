using System;
using System.Collections.Generic;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java
{
	public class JavaServiceModelResponseStatusAmmender
		: ServiceModelResponseStatusAmmender
	{
		public JavaServiceModelResponseStatusAmmender(ServiceModel serviceModel, CodeGenerationOptions options)
			: base(serviceModel, options)
		{
		}

		protected override ServiceClass CreateValueResponseServiceClass(Type type)
		{
			var isNullable = DryNullable.GetUnderlyingType(type) != null;

			if (isNullable)
			{
				type = DryNullable.GetUnderlyingType(type);
			}

			var typeName = TypeSystem.GetPrimitiveName(type);

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

			return new ServiceClass(JavaBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
		}
	}
}
