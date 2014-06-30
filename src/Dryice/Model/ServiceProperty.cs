using Platform.Xml.Serialization;

namespace Dryice.Model
{
	[XmlElement("Property")]
	public class ServiceProperty
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string TypeName { get; set; }
	}
}
