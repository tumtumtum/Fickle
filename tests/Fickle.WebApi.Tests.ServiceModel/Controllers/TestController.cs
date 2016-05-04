using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Fickle.WebApi.Tests.ServiceModel.ServiceModel;

namespace Fickle.WebApi.Tests.ServiceModel.Controllers
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

		[ResponseType(typeof(Person))]
		public IHttpActionResult TestActionResult()
		{
			throw new NotImplementedException();
		}

		[ResponseType(typeof(Person))]
		public HttpResponseMessage TestResponseMessage()
		{
			throw new NotImplementedException();
		}

		[ResponseType(typeof(Person))]
		public async Task<IHttpActionResult> HttpResponseMessageTestAsync()
		{
			throw new NotImplementedException();
		}

		[AcceptVerbs("GET", "HEAD")]
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
