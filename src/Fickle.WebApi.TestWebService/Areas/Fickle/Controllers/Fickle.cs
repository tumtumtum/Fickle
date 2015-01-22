﻿using System.Web.Http;
using System.Web.Mvc;
using Fickle.Ficklefile;
using Fickle.Reflectors;
using Fickle.WebApi;

namespace Fickle.WebApi.TestWebService.Areas.Fickle.Controllers
{
    public class Fickle
		: Controller
    {
		public HttpConfiguration Configuration { get; private set; }

		public Fickle()
            : this(GlobalConfiguration.Configuration)
        {
        }

		public Fickle(HttpConfiguration config)
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