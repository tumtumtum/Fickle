using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Fickle.Expressions;
using Fickle.Model;
using Platform;

namespace Fickle.Generators.Java
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
			string typeName;

			type = type.GetUnwrappedNullableType();

			if (type is FickleListType)
			{
				var elementType = type.GetFickleListElementType();

				typeName = TypeSystem.GetPrimitiveName(elementType) + "?[]";
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

			return new ServiceClass(JavaBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
		}
	}
}
