using System.Collections.Generic;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement("Type")]
	public class ServiceType
	{
		public bool IsPrimitive
		{
			get
			{
				return this.PrimitiveType != null;
			}
		}

		public PrimitiveType? PrimitiveType { get; set; }

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string BaseTypeName { get; set; }

		[XmlElement, XmlListElement(typeof(ServiceTypeProperty))]
		public List<ServiceTypeProperty> Properties { get; set; }

		public static ServiceType CreateServiceType(string name)
		{
			return new ServiceType()
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

			var typedObj = obj as ServiceType;

			if (typedObj == null)
			{
				return false;
			}

			return this.Name.Equals(typedObj.Name) && this.IsPrimitive == typedObj.IsPrimitive;
		}
	}
}
