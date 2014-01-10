using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Enum")]
	public class ServiceEnum
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlElement, XmlListElement(typeof(ServiceEnumValue))]
		public List<ServiceEnumValue> Values { get; set; }
	}
}
