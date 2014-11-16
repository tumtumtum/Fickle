using System;
using System.Collections.Generic;

namespace Fickle.Model
{
	public class ServiceClass
	{
		public string Name { get; set; }

		[ServiceAnnotation]
		public string BaseTypeName { get; set; }

		public List<ServiceProperty> Properties { get; set; }

		public ServiceClass()
		{
		}
		
		public ServiceClass(string name, string baseTypeName, List<ServiceProperty> properties)
		{
			this.Name = name;
			this.BaseTypeName = baseTypeName;
			this.Properties = properties;
		}

		#region Equals

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, this))
			{
				return true;
			}

			var value = obj as ServiceClass;

			return value != null && this.Name.Equals(value.Name);
		}

		#endregion

		public override string ToString()
		{
			return this.Name;
		}
	}
}
