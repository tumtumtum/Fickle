using System;

namespace DryIce.WebApi.TestWebService.ServiceModel
{
	public class User
		: Person
	{
		public string PublicKeyToken { get; set; }
	}
}