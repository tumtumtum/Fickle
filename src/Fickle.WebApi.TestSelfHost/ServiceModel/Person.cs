using System;

namespace Fickle.WebApi.TestSelfHost.ServiceModel
{
	public class Person
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Sex? Sex { get; set; }
	}
}