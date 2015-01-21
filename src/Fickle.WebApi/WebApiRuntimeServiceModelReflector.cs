using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Hosting;
using Fickle.Model;
using Fickle.Reflectors;
using Platform;

namespace Fickle.WebApi
{
	public class WebApiRuntimeServiceModelReflector
		: ServiceModelReflector
	{
		public HttpConfiguration Configuration { get; private set; }
		public ServiceModelReflectionOptions Options { get; private set; }

		public WebApiRuntimeServiceModelReflector(ServiceModelReflectionOptions options, HttpConfiguration configuration)
		{
			this.Options = options;
			this.Configuration = configuration;
		}

		private static bool IsBaseType(Type type)
		{
			return type == null || type == typeof(object) || type == typeof(Enum) || type == typeof(ValueType);
		}

		private static void AddType(ISet<Type> set, Type type)
		{
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

				foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					AddType(set, property.PropertyType);
				}

				type = type.BaseType;
			}
		}

		private static IEnumerable<Type> GetReferencedTypes(IEnumerable<ApiDescription> descriptions)
		{
			var types = new HashSet<Type>();

			foreach (var description in descriptions)
			{
				AddType(types, description.ActionDescriptor.ReturnType);
				
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

			if (typeof(IEnumerable<>).IsAssignableFromIgnoreGenericParameters(type))
			{
				return GetTypeName(type.GetGenericArguments()[0]) + "[]";
			}

			return type.Name;
		}

		public override ServiceModel Reflect()
		{
			var descriptions = Configuration.Services.GetApiExplorer().ApiDescriptions;

			var httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
			var httpRequestContext = httpRequestMessage.Properties[HttpPropertyKeys.RequestContextKey] as HttpRequestContext;

			var url = HttpContext.Current.Request.Url;

			var hostname = url.Host;
			var applicationRoot = httpRequestContext.Configuration.VirtualPathRoot;

			var enums = new List<ServiceEnum>();
			var classes = new List<ServiceClass>();
			var gateways = new List<ServiceGateway>();

			var referencedTypes = GetReferencedTypes(descriptions).ToList();
			var controllers = descriptions.Select(c => c.ActionDescriptor.ControllerDescriptor).ToHashSet();

			foreach (var enumType in referencedTypes.Where(c => c.BaseType == typeof(Enum)))
			{
				var serviceEnum = new ServiceEnum
				{
					Name = GetTypeName(enumType),
					Values = ((int[])enumType.GetEnumValues()).Select(c => new ServiceEnumValue { Name = enumType.GetEnumName(c), Value = c}).ToList()
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

			foreach (var controller in controllers)
			{
				var methods = new List<ServiceMethod>();

				foreach (var api in descriptions.Where(c => c.ActionDescriptor.ControllerDescriptor == controller))
				{
					var formatters = api.ActionDescriptor.ControllerDescriptor.Configuration.Formatters;

					if (!formatters.Any(c => c is JsonMediaTypeFormatter))
					{
						continue;
					}

					var serviceMethod = new ServiceMethod
					{
						Authenticated = api.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>(true).Count > 0,
						Name = api.ActionDescriptor.ActionName,
						Path = StringUriUtils.Combine(applicationRoot, api.RelativePath),
						Returns = GetTypeName(api.ActionDescriptor.ReturnType),
						Format = "json",
						Method = api.HttpMethod.Method.ToLower(),
						Parameters = api.ActionDescriptor.GetParameters().Select(d => new ServiceParameter
						{
							Name = d.ParameterName,
							TypeName = GetTypeName(d.ParameterType)
						}).ToList()
					};

					var bodyParameter = api.ParameterDescriptions.FirstOrDefault(c => c.ParameterDescriptor.ParameterBinderAttribute is FromBodyAttribute);

					if (bodyParameter != null)
					{
						serviceMethod.Content = bodyParameter.Name;
						serviceMethod.ContentServiceParameter = serviceMethod.Parameters.FirstOrDefault(c => c.Name == bodyParameter.Name);
					}

					methods.Add(serviceMethod);
				}

				var serviceGateway = new ServiceGateway
				{
					BaseTypeName = null,
					Name = controller.ControllerName,
					Hostname = hostname,
					Methods = methods
				};

				gateways.Add(serviceGateway);
			}

			return new ServiceModel(new ServiceModelInfo(), enums, classes, gateways);
		}
	}
}
