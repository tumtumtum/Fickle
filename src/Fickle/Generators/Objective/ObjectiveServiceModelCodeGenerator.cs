using System.IO;
using System.IO.Ports;
using System.Linq.Expressions;
using Fickle.Expressions;
using Fickle.Generators.Objective.Binders;
using Platform.VirtualFileSystem;
using Fickle.Model;

namespace Fickle.Generators.Objective
{
	[ServiceModelCodeGenerator("objc", "objective", "objective-c")]
	public class ObjectiveServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		public ObjectiveServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file, options)
		{
		}

		public ObjectiveServiceModelCodeGenerator(TextWriter writer, CodeGenerationOptions options)
			: base(writer, options)
		{
		}

		public ObjectiveServiceModelCodeGenerator(IDirectory directory, CodeGenerationOptions options)
			: base(directory, options)
		{
		}

		public override void Generate(ServiceModel serviceModel)
		{
			serviceModel.ServiceModelInfo.Import(this.Options.ServiceModelInfo);

			base.Generate(serviceModel);
			this.GeneratePodspec(serviceModel);
		}

		protected virtual void GeneratePodspec(ServiceModel serviceModel)
		{
			using (var writer = this.GetTextWriterForFile(serviceModel.ServiceModelInfo.Name + ".podspec"))
			{
				var podspecWriter = new PodspecWriter(writer);

				podspecWriter.Write(serviceModel.ServiceModelInfo);
			}
		}

		protected override ServiceModel ProcessPregeneration(ServiceModel serviceModel)
		{
			serviceModel = new ObjectiveServiceModelResponseAmender(serviceModel, this.Options).Ammend();

			return serviceModel;
		}

		protected override void GenerateClass(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".h"))
			{
				var headerFileExpression = ClassHeaderExpressionBinder.Bind(expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(headerFileExpression);
			}

			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".m"))
			{
				var classFileExpression = ClassSourceExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateGateway(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".h"))
			{
				var headerFileExpression = GatewayHeaderExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(headerFileExpression);
			}

			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".m"))
			{
				var classFileExpression = GatewaySourceExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateEnum(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".h"))
			{
				var enumFileExpression = EnumHeaderExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(enumFileExpression);
			}
		}
	}
}
