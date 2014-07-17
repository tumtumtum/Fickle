using System.Collections.Generic;

namespace Dryice.Model
{
	public class ServiceGateway
	{
		[ServiceAnnotation] 
		public string Name { get; set; }
		
		[ServiceAnnotation] 
		public string Hostname { get; set; }

		[ServiceAnnotation] 
		public string BaseTypeName { get; set; }

		public List<ServiceMethod> Methods { get; set; }
	}
}
