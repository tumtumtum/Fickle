using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement]
	public class ServiceEnumValue
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public int? Value { get; set; }
	}
}
