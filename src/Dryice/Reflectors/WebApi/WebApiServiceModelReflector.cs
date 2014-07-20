using Dryice.Model;

namespace Dryice.Reflectors.WebApi
{
	public class WebApiServiceModelReflector
		: ServiceModelReflector
	{
		public ServiceModelReflectionOptions Options { get; private set; }

		protected WebApiServiceModelReflector(ServiceModelReflectionOptions options)
		{
			this.Options = options;
		}

		public override ServiceModel Reflect()
		{
			return null;
		}
	}
}
