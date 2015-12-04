using System.Collections.Generic;

namespace Fickle.Model
{
	public class ServiceEnum
	{
		[ServiceAnnotation]
		public bool? Flags { get; set; }
		public string Name { get; set; }
		public List<ServiceEnumValue> Values { get; set; }
	}
}
