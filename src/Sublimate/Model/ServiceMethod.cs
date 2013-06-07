using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Method")]
	public class ServiceMethod
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string ReturnTypeName { get; set; }

		[XmlAttribute]
		public string ContentTypeName { get; set; }

		[XmlElement]
		public List<ServiceParameter> Parameters { get; set; }
	}
}
