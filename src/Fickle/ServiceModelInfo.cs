using System.Reflection;

namespace Fickle
{
	public class ServiceModelInfo
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }

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
