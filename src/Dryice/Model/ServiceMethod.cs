using System.Collections.Generic;

namespace Dryice.Model
{
	public class ServiceMethod
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string Format { get; set; }
		public bool Secure { get; set; }
		public bool Authenticated { get; set; }
		public string Method { get; set; }
		public ServiceParameter Content { get; set; }
		public string Returns { get; set; }
		public List<ServiceParameter> Parameters { get; set; }
	}
}
