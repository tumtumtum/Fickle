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
		public string Url { get; set; }

		[XmlAttribute]
		public string Format { get; set; }

		[XmlAttribute]
		public bool Secure { get; set; }

		[XmlAttribute]
		public bool Authenticated { get; set; }

		[XmlAttribute]
		public string Method { get; set; }

		[XmlElement]
		public ServiceParameter Content { get; set; }

		[XmlAttribute]
		public string ReturnTypeName { get; set; }

		[XmlElement]
		public List<ServiceParameter> Parameters { get; set; }
	}
}
