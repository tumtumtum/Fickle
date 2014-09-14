using System;

namespace Fickle.WebApi.TestWebService.ServiceModel
{
	public class User
		: Person
	{
		public string PublicKeyToken { get; set; }
	}
}