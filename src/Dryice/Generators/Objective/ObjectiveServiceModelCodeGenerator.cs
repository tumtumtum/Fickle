using System.IO;
using System.IO.Ports;
using Dryice.Generators.Objective.Binders;
using Platform.VirtualFileSystem;
using Dryice.Model;

namespace Dryice.Generators.Objective
{
	[ServiceModelCodeGenerator("objc", "objective", "objective-c")]
	public class ObjectiveServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		public ObjectiveServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file, options)
		{
		}

		public ObjectiveServiceModelCodeGenerator(TextWriter writer,  CodeGenerationOptions options)
			: base(writer, options)
		{
		}

		public ObjectiveServiceModelCodeGenerator(IDirectory directory,  CodeGenerationOptions options)
			: base(directory, options)
		{
		}

		public override void Generate(ServiceModel serviceModel)
		{	
			serviceModel = new ObjectiveServiceModelResponseStatusAmmender(serviceModel, this.Options).Ammend();

			var codeGenerationContext = new CodeGenerationContext(serviceModel, this.Options);

			var serviceExpressionBuilder = new ServiceExpressionBuilder(serviceModel, this.Options);

			if (this.Options.GenerateClasses)
			{
				foreach (var serviceClass in serviceModel.Classes)
				{
					var classExpression = serviceExpressionBuilder.Build(serviceClass);

					using (var writer = this.GetTextWriterForFile(serviceClass.Name + ".h"))
					{
						var headerFileExpression = ClassHeaderExpressionBinder.Bind(classExpression);

						var codeGenerator = new ObjectiveCodeGenerator(writer);

						codeGenerator.Generate(headerFileExpression);
					}

					using (var writer = this.GetTextWriterForFile(serviceClass.Name + ".m"))
					{
						var classFileExpression = ClassSourceExpressionBinder.Bind(serviceModel, classExpression);

						var codeGenerator = new ObjectiveCodeGenerator(writer);

						codeGenerator.Generate(classFileExpression);
					}
				}
			}

			if (this.Options.GenerateGateways)
			{
				foreach (var serviceGateway in serviceModel.Gateways)
				{
					var gatewayExpression = serviceExpressionBuilder.Build(serviceGateway);

					using (var writer = this.GetTextWriterForFile(serviceGateway.Name + ".h"))
					{
						var headerFileExpression = GatewayHeaderExpressionBinder.Bind(codeGenerationContext, gatewayExpression);

						var codeGenerator = new ObjectiveCodeGenerator(writer);

						codeGenerator.Generate(headerFileExpression);
					}

					using (var writer = this.GetTextWriterForFile(serviceGateway.Name + ".m"))
					{
						var classFileExpression = GatewaySourceExpressionBinder.Bind(codeGenerationContext, gatewayExpression);

						var codeGenerator = new ObjectiveCodeGenerator(writer);

						codeGenerator.Generate(classFileExpression);
					}
				}
			}
		}
	}
}
