using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Gateway")]
	public class ServiceGateway
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlElement]
		public List<ServiceMethod> Methods { get; set; }
	}
}
