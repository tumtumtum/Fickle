using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Fickle.Ficklefile;
using Fickle.Reflectors;

namespace Fickle.WebApi.TestSelfHost.Controllers
{
	public class FickleController
		: ApiController
	{
		[Route("Fickle")]
		[HttpGet]
		public HttpResponseMessage Fickle()
		{
			var options = new ServiceModelReflectionOptions
			{
				ControllersTypesToIgnore = new[] { this.GetType() }
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
