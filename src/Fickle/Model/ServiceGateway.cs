using System.Collections.Generic;

namespace Fickle.Model
{
	public class ServiceGateway
	{
		public string Name { get; set; }
		
		[ServiceAnnotation] 
		public string Hostname { get; set; }

		[ServiceAnnotation]
		public string BaseTypeName { get; set; }

		public List<ServiceMethod> Methods { get; set; }
	}
}
