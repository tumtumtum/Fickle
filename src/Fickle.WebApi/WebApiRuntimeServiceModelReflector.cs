using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using Fickle.Ficklefile;
using Fickle.Model;
using Fickle.Reflectors;
using Platform;

namespace Fickle.WebApi
{
	public class WebApiRuntimeServiceModelReflector
		: ServiceModelReflector
	{
		public static string[] ReservedKeywords => new[] { "enum", "class", "gateway" };

		private readonly Assembly referencingAssembly;
		private readonly string hostname;
		private readonly HttpConfiguration configuration;
		private readonly ServiceModelReflectionOptions options;
		
		public WebApiRuntimeServiceModelReflector(ServiceModelReflectionOptions options, HttpConfiguration configuration, Assembly referencingAssembly, string hostname)
		{
			this.referencingAssembly = referencingAssembly;
			this.hostname = hostname;
			this.options = options;
			this.configuration = configuration;
		}

		private static bool IsBaseType(Type type)
		{
			return type == null || type == typeof(object) || type == typeof(Enum) || type == typeof(ValueType) || type == typeof(Array) || type == typeof(IEnumerable) || (type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}

		private static void AddType(ISet<Type> set, Type type)
		{
			if (type == null)
			{
				return;
			}

			if (Nullable.GetUnderlyingType(type) != null)
			{
				type = Nullable.GetUnderlyingType(type);
			}

			if (set.Contains(type))
			{
				return;
			}

			if (IsBaseType(type))
			{
				return;
			}

			while (!IsBaseType(type))
			{
				set.Add(type);

				if (typeof(IEnumerable<>).IsAssignableFromIgnoreGenericParameters(type))
				{
					var elementType = type.GetSequenceElementType();

					if (elementType != null)
					{
						AddType(set, elementType);
					}
				}

				foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					AddType(set, property.PropertyType);
				}

				type = type.BaseType;
			}
		}

		private IEnumerable<Type> GetReferencedTypes(IEnumerable<ApiDescription> descriptions)
		{
			var types = new HashSet<Type>();

			var attributes = this.referencingAssembly.GetCustomAttributes<FickleIncludeTypeAttribute>();

			foreach (var attribute in attributes.Where(attribute => attribute.Type != null))
			{
				AddType(types, attribute.Type);

				if (attribute.IncludeRelatives)
				{
					var s = attribute.Type.Namespace + ".";

                    foreach (var type in attribute.Type.Assembly.GetTypes()
						.Where(c => c != attribute.Type)
						.Where(c => c.Namespace == attribute.Type.Namespace
							|| c.Namespace != null && c.Namespace.StartsWith(s)))
					{
						AddType(types, type);
					}
				}
			}

			foreach (var description in descriptions)
			{
				AddType(types, description.ResponseDescription.ResponseType ?? description.ResponseDescription.DeclaredType);
				
				foreach (var type in description.ParameterDescriptions.Select(c => c.ParameterDescriptor.ParameterType))
				{
					AddType(types, type);
				}
			}

			return types.Where(c => c != null);
		}

		private static string GetTypeName(Type type)
		{
			if (type == null)
			{
				return null;
			}
			
			if (TypeSystem.IsPrimitiveType(type))
			{
				if (type.GetUnwrappedNullableType().IsEnum)
				{
					return TypeSystem.GetPrimitiveName(type);
                }
				else
				{
					return TypeSystem.GetPrimitiveName(type).ToLower();
				}
			}

			if (type == typeof(object))
			{
				return null;
			}

			if (typeof (IEnumerable<>).IsAssignableFromIgnoreGenericParameters(type))
			{
				Type enumerableType;

				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEnumerable<>))
				{
					enumerableType = type;
				}
				else
				{
					enumerableType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
				}

				if (enumerableType != null)
				{
					return GetTypeName(enumerableType.GetGenericArguments()[0]) + "[]";
				}
			}

			return type.Name;
		}

		public override ServiceModel Reflect()
		{
			var descriptions = configuration.Services.GetApiExplorer().ApiDescriptions.AsEnumerable().ToList();

			if (this.options.ControllersTypesToIgnore != null)
			{
				descriptions = descriptions.Where(x => !this.options.ControllersTypesToIgnore.Contains(x.ActionDescriptor.ControllerDescriptor.ControllerType)).ToList();
			}

			var enums = new List<ServiceEnum>();
			var classes = new List<ServiceClass>();
			var gateways = new List<ServiceGateway>();

			var referencedTypes = GetReferencedTypes(descriptions).ToList();
			var controllers = descriptions.Select(c => c.ActionDescriptor.ControllerDescriptor).ToHashSet();

			var serviceModelInfo = new ServiceModelInfo();

			foreach (var enumType in referencedTypes.Where(c => c.BaseType == typeof(Enum)))
			{
				var serviceEnum = new ServiceEnum
				{
					Name = GetTypeName(enumType),
					Values = Enum.GetValues(enumType).Cast<int>().ToArray().Select(c => new ServiceEnumValue { Name = enumType.GetEnumName(c), Value = c}).ToList()
				};

				enums.Add(serviceEnum);
			}

			foreach (var type in referencedTypes
				.Where(TypeSystem.IsNotPrimitiveType)
				.Where(c => c.BaseType != typeof(Enum))
				.Where(c => !c.IsInterface)
				.Where(c => !typeof(IList<>).IsAssignableFromIgnoreGenericParameters(c)))
			{
				var baseTypeName = GetTypeName(type.BaseType);
				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Select(c => new ServiceProperty
					{
						Name = c.Name,
						TypeName = GetTypeName(c.PropertyType)
					}).ToList();

				var serviceClass = new ServiceClass
				{
					Name = GetTypeName(type),
					BaseTypeName = baseTypeName,
 					Properties = properties
				};

				classes.Add(serviceClass);
			}

			var allowedMethods = new HashSet<string>(new [] { "GET", "POST" }, StringComparer.InvariantCultureIgnoreCase);

			foreach (var controller in controllers)
			{
				var methods = new List<ServiceMethod>();

				foreach (var api in descriptions.Where(c => c.ActionDescriptor.ControllerDescriptor == controller)
					.Where(c => allowedMethods.Contains(c.HttpMethod.Method)))
				{
					var formatters = api.ActionDescriptor.ControllerDescriptor.Configuration.Formatters;
					var returnType = api.ResponseDescription.ResponseType ?? api.ResponseDescription.DeclaredType;

					if (!formatters.Any(c => c is JsonMediaTypeFormatter))
					{
						returnType = typeof(string);
					}

					var parameters = api.ParameterDescriptions.Select(d => new ServiceParameter
					{
						Name = d.ParameterDescriptor.ParameterName,
						TypeName = GetTypeName(d.ParameterDescriptor.ParameterType)
					}).ToList();

					ServiceParameter contentServiceParameter = null;
					var contentParameter = api.ParameterDescriptions.SingleOrDefault(c => c.Source == ApiParameterSource.FromBody);

					var uniqueNameMaker = new UniqueNameMaker(c => api.ParameterDescriptions.Any(d => d.Name.EqualsIgnoreCase(c)));

					if (contentParameter == null
						&& api.HttpMethod.Method.EqualsIgnoreCaseInvariant("POST")
						&& api.ActionDescriptor.GetCustomAttributes<NoBodyAttribute>().Count == 0)
					{
						contentServiceParameter = new ServiceParameter { Name = uniqueNameMaker.Make("content"), TypeName = GetTypeName(typeof(byte[])) };

						parameters.Add(contentServiceParameter);
					}
					else if (contentParameter != null)
					{
						contentServiceParameter = new ServiceParameter { Name = contentParameter.Name, TypeName = GetTypeName(contentParameter.ParameterDescriptor.ParameterType) };
					}

					var serviceMethod = new ServiceMethod
					{
						Authenticated = api.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>(true).Count > 0,
						Name = api.ActionDescriptor.ActionName,
						Path = StringUriUtils.Combine(this.configuration.VirtualPathRoot, api.RelativePath),
						Returns = GetTypeName(returnType),
						ReturnFormat = "json",
						Method = api.HttpMethod.Method.ToLower(),
						Parameters = parameters
					};
					
					if (contentServiceParameter != null)
					{
						serviceMethod.Content = contentServiceParameter.Name;
						serviceMethod.ContentServiceParameter = contentServiceParameter;
					}
					
					methods.Add(serviceMethod);
				}

				var serviceNameSuffix = "Service";
				var attribute = this.referencingAssembly.GetCustomAttribute<FickleSdkInfoAttribute>();

				if (attribute != null)
				{
					serviceNameSuffix = attribute.ServiceNameSuffix ?? serviceNameSuffix;
					serviceModelInfo.Name = attribute.Name ?? serviceModelInfo.Name;
					serviceModelInfo.Summary = attribute.Summary ?? serviceModelInfo.Summary;
					serviceModelInfo.Author = attribute.Author ?? serviceModelInfo.Author;
					serviceModelInfo.Version = attribute.Version ?? serviceModelInfo.Version;
				}

				var serviceGateway = new ServiceGateway
				{
					BaseTypeName = null,
					Name = controller.ControllerName + serviceNameSuffix,
					Hostname = hostname,
					Methods = methods
				};

				gateways.Add(serviceGateway);
			}

			return new ServiceModel(serviceModelInfo, enums, classes, gateways);
		}
	}
}
