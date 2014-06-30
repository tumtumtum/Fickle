using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Dryice.Model
{
	[XmlElement("Gateway")]
	public class ServiceGateway
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Url { get; set; }

		[XmlElement]
		public List<ServiceMethod> Methods { get; set; }
	}
}
