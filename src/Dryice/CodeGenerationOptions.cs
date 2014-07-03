namespace Dryice
{
	public class CodeGenerationOptions
	{
		public static readonly CodeGenerationOptions Default = new CodeGenerationOptions();

		public string BaseTypeTypeName { get; set; }
		public string BaseGatewayTypeName { get; set; }
		public bool GenerateClasses { get; set; }
		public bool GenerateGateways { get; set; }
		public string ServiceClientTypeName { get; set; }

		public CodeGenerationOptions()
		{
			this.GenerateClasses = true;
			this.GenerateGateways = true;
		}
	}
}
