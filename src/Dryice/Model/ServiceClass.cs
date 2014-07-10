using System.Collections.Generic;

namespace Dryice.Model
{
	public class ServiceClass
	{
		public string Name { get; set; }
		public string BaseTypeName { get; set; }
		public List<ServiceProperty> Properties { get; set; }

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

		#endregion
	}
}
