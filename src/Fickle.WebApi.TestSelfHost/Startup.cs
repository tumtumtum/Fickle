using System.Web.Http;
using Fickle.WebApi.TestSelfHost;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Fickle.WebApi.TestSelfHost
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var config = new HttpConfiguration();

			WebApiConfig.Register(config);

			app.UseWebApi(config);
		}
	}
}