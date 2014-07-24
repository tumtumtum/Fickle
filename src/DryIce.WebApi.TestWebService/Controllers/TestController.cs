using System;
using System.Web.Http;
using DryIce.WebApi.TestWebService.ServiceModel;

namespace DryIce.WebApi.TestWebService.Controllers
{
	public class TestController
		: ApiController
	{
		private readonly Random random = new Random();

		[AcceptVerbs("GET")]
	    public int AddOne(int x)
	    {
		    return x + 1;
	    }


		[AcceptVerbs("GET")]
		public int Random()
		{
			return random.Next();
		}

		[AcceptVerbs("GET")]
		public User GetUser(Guid id)
		{
			return new User
			{
				Id = id
			};
		}

		[AcceptVerbs("GET")]
		public void AddUser(User user)
		{
		}
    }
}
