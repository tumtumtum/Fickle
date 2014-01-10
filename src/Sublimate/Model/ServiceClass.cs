using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Class")]
	public class ServiceClass
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string BaseTypeName { get; set; }

		[XmlElement, XmlListElement(typeof(ServiceProperty))]
		public List<ServiceProperty> Properties { get; set; }

		public static ServiceClass CreateServiceType(string name)
		{
			return new ServiceClass()
			{
				Name = name
			};
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}

			var typedObj = obj as ServiceClass;

			if (typedObj == null)
			{
				return false;
			}

			return this.Name.Equals(typedObj.Name);
		}
	}
}
