using System;

namespace Fickle.WebApi.Tests.ServiceModel.ServiceModel
{
	public class Person
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Sex? Sex { get; set; }
		public Person[] Friends { get; set; }

		[FickleExclude]
		public string Excluded { get; set; }

		[FickleExclude]
		public Excluded ShouldNotBeIncluded { get; set; }
	}
}