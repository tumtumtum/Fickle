using Platform.Xml.Serialization;

namespace Dryice.Model
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
