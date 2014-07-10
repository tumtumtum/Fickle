using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Dryice.Model
{
	public class ServiceModel
	{
		public ReadOnlyCollection<ServiceEnum> Enums { get; private set; }
		public ReadOnlyCollection<ServiceClass> Classes { get; private set; }
		public ReadOnlyCollection<ServiceGateway> Gateways { get; private set; }

		private Dictionary<string, Type> serviceTypesByName;

		public ServiceModel(IEnumerable<ServiceEnum> enums, IEnumerable<ServiceClass> classes, IEnumerable<ServiceGateway> gateways)
		{
			this.Enums = enums.ToReadOnlyCollection();
			this.Classes = classes.ToReadOnlyCollection();
			this.Gateways = gateways.ToReadOnlyCollection();
		}

		public virtual Type GetTypeFromName(string name)
		{
			var list = false;

			if (name.EndsWith("[]"))
			{
				list = true;
				name = name.Substring(0, name.Length - 2);
			}

			var type = TypeSystem.GetPrimitiveType(name);

			if (type == null)
			{
				type = this.GetServiceType(name);
			}

			if (list)
			{
				return MakeListType(type);
			}
			else
			{
				return type;
			}
		}

		private readonly Dictionary<Type, Type> listTypesByElementType = new Dictionary<Type, Type>();

		private Type MakeListType(Type elementType)
		{
			Type value;
			
			if (!listTypesByElementType.TryGetValue(elementType, out value))
			{
				value = new DryListType(elementType);
			}

			return value;
		}

		public virtual ServiceClass GetServiceClass(string name)
		{
			return this.GetServiceClass(this.GetServiceType(name));
		}

		public virtual ServiceClass GetServiceClass(Type type)
		{
			var dryType = type as DryType;

			if (dryType == null)
			{
				return null;
			}

			return dryType.ServiceClass;
		}

		private void CreateIndex()
		{
			this.serviceTypesByName = this.Classes.Select(c => (object)c).Concat(this.Enums ?? Enumerable.Empty<object>()).Select(c => c is ServiceEnum ? (Type)new DryType((ServiceEnum)c, this) : (Type)new DryType((ServiceClass)c, this)).Distinct().ToDictionary(c => c.Name, c => c, StringComparer.InvariantCultureIgnoreCase);

			foreach (DryType type in this.serviceTypesByName.Values)
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
						type.SetBaseType(new DryType(type.ServiceClass.BaseTypeName));
					}
				}
				else
				{
					type.SetBaseType(typeof(object));
				}
			}
		}

		public virtual Type GetServiceType(string name)
		{
			if (this.serviceTypesByName == null)
			{
				this.CreateIndex();
			}

			Type retval;

			if (this.serviceTypesByName.TryGetValue(name, out retval))
			{
				return retval;
			}

			this.CreateIndex();

			if (this.serviceTypesByName.TryGetValue(name, out retval))
			{
				return retval;
			}

			throw new InvalidOperationException("Type not found: " + name);
		}
	}
}
