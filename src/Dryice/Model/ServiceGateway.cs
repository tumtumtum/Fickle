using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Dryice.Model
{
	public class ServiceGateway
	{
		public string Name { get; set; }
		public string Hostname { get; set; }
		public string BaseTypeName { get; set; }
		public List<ServiceMethod> Methods { get; set; }
	}
}
