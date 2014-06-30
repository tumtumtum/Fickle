using Platform.Xml.Serialization;

namespace Dryice.Model
{
	[XmlElement("VariableExpression")]
	public class ServiceParameter
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string TypeName { get; set; }
	}
}
