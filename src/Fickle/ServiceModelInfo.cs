using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fickle.Model;

namespace Fickle
{
	public class ServiceModelInfo
	{
		[ServiceAnnotation]
		public string Name { get; set; }

		[ServiceAnnotation]
		public string Version { get; set; }

		[ServiceAnnotation]
		public string Summary { get; set; }

		[ServiceAnnotation]
		public string Author { get; set; }

		public Dictionary<string, string> ExtendedValues { get; set; }

		public ServiceModelInfo()
		{
			this.ExtendedValues = new Dictionary<string, string>();
		}

		public bool HasAnyNonNullValues()
		{
			return this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(c => c.GetCustomAttribute<ServiceAnnotationAttribute>() != null).Select(property => property.GetValue(this)).Any(value => value != null)
			       || this.ExtendedValues.Any(c => c.Value != null);
		}

		public void Import(ServiceModelInfo serviceModelInfo)
		{
			foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(c => c.GetCustomAttribute<ServiceAnnotationAttribute>() != null))
			{
				var value = property.GetValue(serviceModelInfo);

				if (value != null)
				{
					property.SetValue(this, value);
				}
			}

			foreach (var value in this.ExtendedValues.Where(value => value.Value != null))
			{
				this.ExtendedValues[value.Key] = value.Value;
			}
		}
	}
}
