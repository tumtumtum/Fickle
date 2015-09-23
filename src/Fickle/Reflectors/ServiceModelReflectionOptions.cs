using System;

namespace Fickle.Reflectors
{
	public class ServiceModelReflectionOptions
	{
		public string OutputPath { get; set; }
		public bool OutputToConsole { get; set; }
		public string[] Namespaces { get; set; }
		public string[] ModelAssemblyPaths { get; set; }
		public string[] GatewayAssemblyPaths { get; set; }
		public Type[] ControllersTypesToIgnore { get; set; }
	}
}
