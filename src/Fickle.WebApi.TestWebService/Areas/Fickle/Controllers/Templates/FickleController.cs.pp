using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Fickle.Ficklefile;
using Fickle.Reflectors;

namespace $rootnamespace$.Areas.Fickle.Controllers
{
	public class FickleController
		: ApiController
	{
		private static readonly Type[] controllersToIgnore;

		static FickleController()
		{
			var list = new List<Type> { typeof(FickleController) };
			var attributes = typeof(FickleController).Assembly.GetCustomAttributes(typeof(FickleExcludeControllerAttribute), true);

			list.AddRange(attributes.Cast<FickleExcludeControllerAttribute>().Select(c => c.Type));

			controllersToIgnore = list.ToArray();
		}

		[HttpGet]
		[Route("fickle")]
		public HttpResponseMessage Fickle()
		{
			var options = new ServiceModelReflectionOptions
			{
				ControllersTypesToIgnore = controllersToIgnore
			};

			var reflector = new WebApiRuntimeServiceModelReflector(options, this.Configuration, this.GetType().Assembly, Request.RequestUri.Host);
			var serviceModel = reflector.Reflect();

			var content = new PushStreamContent(
				(stream, httpContent, transportContext) =>
				{
					using (var streamWriter = new StreamWriter(stream))
					{
						var writer = new FicklefileWriter(streamWriter);
						writer.Write(serviceModel);
					}
				},
				new MediaTypeHeaderValue("text/fickle"));

			var response = new HttpResponseMessage
			{
				Content = content
			};

			return response;
		}
	}
}
