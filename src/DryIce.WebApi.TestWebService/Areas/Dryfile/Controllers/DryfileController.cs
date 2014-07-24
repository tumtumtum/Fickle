using System.Web.Http;
using System.Web.Mvc;
using Dryice.Dryfile;
using Dryice.Reflectors;
using Dryice.Reflectors.WebApiRuntime;

namespace DryIce.WebApi.TestWebService.Areas.Dryfile.Controllers
{
    public class DryfileController
		: Controller
    {
		public HttpConfiguration Configuration { get; private set; }

		public DryfileController()
            : this(GlobalConfiguration.Configuration)
        {
        }

		public DryfileController(HttpConfiguration config)
        {
            Configuration = config;
        }

		public ActionResult Index()
		{
			var reflector = new WebApiRuntimeServiceModelReflector(new ServiceModelReflectionOptions(), this.Configuration);
			var serviceModel = reflector.Reflect();
			var writer = new DryFileWriter(this.Response.Output);

			this.Response.ContentType = "text/dryfile";
			
			writer.Write(serviceModel);

			return new EmptyResult();;
		}
    }
}
