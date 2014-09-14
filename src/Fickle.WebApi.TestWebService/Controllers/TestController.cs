using System;
using System.Web.Http;
using Fickle.WebApi.TestWebService.ServiceModel;

namespace Fickle.WebApi.TestWebService.Controllers
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
		public Sex? GetUserSex(Guid userId)
		{
			return Sex.Female;
		}

		[AcceptVerbs("GET")]
		public string GetUserName(Guid? id)
		{
			return "Bob";
		}

		[Authorize]
		[AcceptVerbs("GET")]
		public int Random()
		{
			return random.Next();
		}

		[AcceptVerbs("GET")]
		public User GetUser(Guid? id)
		{
			return new User
			{
				Id = Guid.NewGuid()
			};
		}

		[AcceptVerbs("GET")]
		public void AddUser(User user)
		{
		}
    }
}
