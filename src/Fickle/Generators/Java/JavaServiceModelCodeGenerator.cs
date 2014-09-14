using System.IO;
using System.IO.Ports;
using System.Linq.Expressions;
using Fickle.Expressions;
using Fickle.Generators.Java.Binders;
using Platform.VirtualFileSystem;
using Fickle.Model;

namespace Fickle.Generators.Java
{
	[ServiceModelCodeGenerator("java")]
	public class JavaServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		public JavaServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file, options)
		{
		}

		public JavaServiceModelCodeGenerator(TextWriter writer, CodeGenerationOptions options)
			: base(writer, options)
		{
		}

		public JavaServiceModelCodeGenerator(IDirectory directory, CodeGenerationOptions options)
			: base(directory, options)
		{
		}

		protected override ServiceModel ProcessPregeneration(ServiceModel serviceModel)
		{
			serviceModel = new JavaServiceModelResponseAmender(serviceModel, this.Options).Ammend();

			return serviceModel;
		}

		protected override void GenerateClass(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".java"))
			{
				var classFileExpression = ClassExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new JavaCodeGenerator(writer);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateGateway(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".java"))
			{
				var classFileExpression = GatewayExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new JavaCodeGenerator(writer);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateEnum(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".java"))
			{
				var enumFileExpression = EnumExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new JavaCodeGenerator(writer);

				codeGenerator.Generate(enumFileExpression);
			}
		}
	}
}
