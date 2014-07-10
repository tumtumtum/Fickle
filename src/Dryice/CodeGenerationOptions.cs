namespace Dryice
{
	public class CodeGenerationOptions
	{
		public static readonly CodeGenerationOptions Default = new CodeGenerationOptions();

		public string BaseGatewayTypeName { get; set; }
		public bool GenerateClasses { get; set; }
		public bool GenerateGateways { get; set; }
		public string ServiceClientTypeName { get; set; }
		public string ResponseStatusTypeName { get; set; }
		public string ResponseStatusPropertyName { get; set; }

		public CodeGenerationOptions()
		{
			this.GenerateClasses = true;
			this.GenerateGateways = true;
			this.ResponseStatusTypeName = "ResponseStatus";
			this.ResponseStatusPropertyName = "ResponseStatus";
		}
	}
}
