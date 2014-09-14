using System.Web;
using System.Web.Mvc;

namespace Fickle.WebApi.TestWebService
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
