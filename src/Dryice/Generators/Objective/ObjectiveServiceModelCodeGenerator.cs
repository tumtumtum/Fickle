using System.IO;
using Platform.VirtualFileSystem;
using Dryice.Model;

namespace Dryice.Generators.Objective
{
	[ServiceModelCodeGenerator("objc", "objective", "objective-c")]
	public class ObjectiveServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		public ObjectiveServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file)
		{
		}

		public ObjectiveServiceModelCodeGenerator(TextWriter writer,  CodeGenerationOptions options)
			: base(writer)
		{
		}

		public ObjectiveServiceModelCodeGenerator(IDirectory directory,  CodeGenerationOptions options)
			: base(directory)
		{
		}

		public override void Generate(ServiceModel serviceModel)
		{
			var serviceExpressionBuilder = new ServiceExpressionBuilder(serviceModel);
			
			foreach (var serviceClass in serviceModel.Classes)
			{
				var classExpression = serviceExpressionBuilder.Build(serviceClass);

				using (var writer = this.GetTextWriterForFile(serviceClass.Name + ".h"))
				{
					var headerFileExpression = ObjectiveClassHeaderExpressionBinder.Bind(classExpression);

					var codeGenerator = new ObjectiveCodeGenerator(writer);

					codeGenerator.Generate(headerFileExpression);
				}

				using (var writer = this.GetTextWriterForFile(serviceClass.Name + ".m"))
				{
					var classFileExpression = ObjectiveClassExpressionBinder.Bind(serviceModel, classExpression);

					var codeGenerator = new ObjectiveCodeGenerator(writer);

					codeGenerator.Generate(classFileExpression);
				}
			}
		}
	}
}
