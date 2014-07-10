using System;
using System.Collections.Generic;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Objective
{
	public class ObjectiveServiceModelResponseStatusAmmender
		: ServiceModelResponseStatusAmmender
	{
		public ObjectiveServiceModelResponseStatusAmmender(ServiceModel serviceModel, CodeGenerationOptions options)
			: base(serviceModel, options)
		{
		}

		protected override ServiceClass CreateValueResponseServiceClass(Type type)
		{
			var isNullable = Nullable.GetUnderlyingType(type) != null;

			if (isNullable)
			{
				type = Nullable.GetUnderlyingType(type);
			}

			string typeName;

			if (type.IsNumericType())
			{
				typeName = "int?";
			}
			else if (isNullable)
			{
				typeName = TypeSystem.GetPrimitiveName(type) + "?";
			}
			else
			{
				typeName = TypeSystem.GetPrimitiveName(type);
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

			return new ServiceClass(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
		}
	}
}
