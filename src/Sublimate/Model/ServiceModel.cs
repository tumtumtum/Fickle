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
		public List<ServiceClass> Classes { get; set; }

		[XmlElement]
		public List<ServiceEnum> Enums { get; set; }

		[XmlElement]
		public List<ServiceGateway> Gateways { get; set; }

		private Dictionary<string, Type> serviceTypesByName;

		public virtual Type GetServiceType(string name)
		{
			if (this.serviceTypesByName == null)
			{
				this.serviceTypesByName = this.Classes.Select(c => (object)c).Concat(this.Enums).Select(c => c is ServiceEnum ? (Type)new SublimateType((ServiceEnum)c) : (Type)new SublimateType((ServiceClass)c)).ToDictionary(c => c.Name, c => c, StringComparer.InvariantCultureIgnoreCase);

				foreach (SublimateType type in this.serviceTypesByName.Values)
				{
					if (type.ServiceClass != null && !string.IsNullOrEmpty(type.ServiceClass.BaseTypeName))
					{
						Type baseType;

						if (this.serviceTypesByName.TryGetValue(type.ServiceClass.BaseTypeName, out baseType))
						{
							type.SetBaseType(baseType);
						}
						else
						{
							type.SetBaseType(new SublimateType(type.ServiceClass.BaseTypeName));
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
