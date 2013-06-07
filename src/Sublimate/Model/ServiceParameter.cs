using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Parameter")]
	public class ServiceParameter
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string TypeName { get; set; }
	}
}
