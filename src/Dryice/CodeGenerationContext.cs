using Dryice.Model;

namespace Dryice
{
	public class CodeGenerationContext
	{
		public ServiceModel ServiceModel { get; private set; }
		public CodeGenerationOptions Options { get; private set; }

		public CodeGenerationContext(ServiceModel serviceModel, CodeGenerationOptions options)
		{
			this.Options = options;
			this.ServiceModel = serviceModel;
		}
	}
}
