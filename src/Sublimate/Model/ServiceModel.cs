using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Platform.Xml.Serialization;

namespace Sublimate.Model
{
	[XmlElement]
	public class ServiceModel
	{
		[XmlElement]
		public List<ServiceType> Types { get; set; }

		[XmlElement]
		public List<ServiceGateway> Gateways { get; set; }

		private Dictionary<string, ServiceType> serviceTypesByName;

		public virtual ServiceType GetServiceType(string name)
		{
			if (this.serviceTypesByName == null)
			{
				this.serviceTypesByName = this.Types.ToDictionary(c => c.Name, c => c, StringComparer.InvariantCultureIgnoreCase);

				foreach (var value in Enum.GetValues(typeof(PrimitiveType)))
				{
					var enumValueName = Enum.GetName(typeof(PrimitiveType), value);

					this.serviceTypesByName[enumValueName] = new ServiceType
					{
						Name = enumValueName,
						PrimitiveType = (PrimitiveType)value
					};
				}
			}

			ServiceType serviceType;

			if (this.serviceTypesByName.TryGetValue(name, out serviceType))
			{
				return serviceType;
			}

			throw new InvalidOperationException("Type not found: " + name);
		}
	}
}
