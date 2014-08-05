using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java
{
	public class JavaServiceModelResponseAmender
		: ServiceModelResponseAmender
	{
		public JavaServiceModelResponseAmender(ServiceModel serviceModel, CodeGenerationOptions options)
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

			var typeName = TypeSystem.GetPrimitiveName(type, true);

			var properties = new List<ServiceProperty>
			{
				new ServiceProperty()
				{
					Name = options.ResponseStatusPropertyName,
					TypeName = options.ResponseStatusTypeName
				}
			};

			if (type != typeof(void))
			{
				properties.Add
				(
					new ServiceProperty()
					{
						Name = "Value",
						TypeName = typeName
					}
				);
			}

			return new ServiceClass(JavaBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
		}
	}
}
