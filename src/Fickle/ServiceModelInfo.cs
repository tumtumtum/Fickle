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

		public bool HasAnyNonNullValues { get { return this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(property => property.GetValue(this)).Any(value => value != null); } }

		public void Import(ServiceModelInfo serviceModelInfo)
		{
			foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				var value = property.GetValue(serviceModelInfo);

				if (value != null)
				{
					property.SetValue(this, value);
				}
			}
		}
	}
}
