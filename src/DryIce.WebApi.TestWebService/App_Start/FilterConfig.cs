using System.Web;
using System.Web.Mvc;

namespace DryIce.WebApi.TestWebService
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
