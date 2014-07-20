using System.Web.Http;

namespace DryIce.WebApi.TestWebService.Controllers
{
	public class TestController
		: ApiController
    {
		[AcceptVerbs("GET")]
	    public int AddOne(int x)
	    {
		    return x + 1;
	    }
    }
}
