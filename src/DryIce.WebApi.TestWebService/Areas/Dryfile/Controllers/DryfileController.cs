using System.Web.Http;
using System.Web.Mvc;
using Fickle.Dryfile;
using Fickle.Reflectors;
using Fickle.Reflectors.WebApiRuntime;

namespace Fickle.WebApi.TestWebService.Areas.Dryfile.Controllers
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
