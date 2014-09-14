using System.Web.Http;
using System.Web.Mvc;
using Fickle.Ficklefile;
using Fickle.Reflectors;
using Fickle.Reflectors.WebApiRuntime;

namespace Fickle.WebApi.TestWebService.Areas.Ficklefile.Controllers
{
    public class FicklefileController
		: Controller
    {
		public HttpConfiguration Configuration { get; private set; }

		public FicklefileController()
            : this(GlobalConfiguration.Configuration)
        {
        }

		public FicklefileController(HttpConfiguration config)
        {
            this.Configuration = config;
        }

		public ActionResult Index()
		{
			var reflector = new WebApiRuntimeServiceModelReflector(new ServiceModelReflectionOptions(), this.Configuration);
			var serviceModel = reflector.Reflect();
			var writer = new FicklefileWriter(this.Response.Output);

			this.Response.ContentType = "text/fickle";
			
			writer.Write(serviceModel);

			return new EmptyResult();;
		}
    }
}
