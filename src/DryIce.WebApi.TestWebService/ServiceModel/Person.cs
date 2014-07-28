using System;

namespace DryIce.WebApi.TestWebService.ServiceModel
{
	public class Person
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Sex? Sex { get; set; }
	}
}