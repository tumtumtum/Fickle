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

		private Dictionary<string, Type> serviceTypesByName;

		public virtual Type GetServiceType(string name)
		{
			if (this.serviceTypesByName == null)
			{
				this.serviceTypesByName = this.Types.Select(c => (Type)new SublimateType(c)).ToDictionary(c => c.Name, c => c, StringComparer.InvariantCultureIgnoreCase);

				foreach (SublimateType type in this.serviceTypesByName.Values)
				{
					if (!string.IsNullOrEmpty(type.ServiceType.BaseTypeName))
					{
						Type baseType;

						if (this.serviceTypesByName.TryGetValue(type.ServiceType.BaseTypeName, out baseType))
						{
							type.SetBaseType(baseType);
						}
						else
						{
							type.SetBaseType(new SublimateType(type.ServiceType.BaseTypeName));
						}
					}
					else
					{
						type.SetBaseType(typeof(object));
					}
				}
			}

			Type retval;

			if (this.serviceTypesByName.TryGetValue(name, out retval))
			{
				return retval;
			}

			throw new InvalidOperationException("Type not found: " + name);
		}
	}
}
