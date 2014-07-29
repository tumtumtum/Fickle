﻿using System;
using System.Collections.Generic;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Objective
{
	public class ObjectiveServiceModelResponseAmender
		: ServiceModelResponseAmender
	{
		public ObjectiveServiceModelResponseAmender(ServiceModel serviceModel, CodeGenerationOptions options)
			: base(serviceModel, options)
		{
		}

		protected override ServiceClass CreateValueResponseServiceClass(Type type)
		{
			var unwrappedType = type.GetUnwrappedNullableType();
			
			string typeName;

			if (unwrappedType.IsNumericType() || unwrappedType == typeof(bool) || (unwrappedType.IsEnum && type.IsNullable()))
			{
				typeName = "int?";
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

			return new ServiceClass(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
		}
	}
}
