using System.IO;
using System.IO.Ports;
using System.Linq.Expressions;
using Fickle.Expressions;
using Fickle.Generators.Javascript.Binders;
using Platform.VirtualFileSystem;
using Fickle.Model;

namespace Fickle.Generators.Javascript
{
	[ServiceModelCodeGenerator("javascript")]
	public class JavascriptServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		public JavascriptServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file, options)
		{
		}

		public JavascriptServiceModelCodeGenerator(TextWriter writer, CodeGenerationOptions options)
			: base(writer, options)
		{
		}

		public JavascriptServiceModelCodeGenerator(IDirectory directory, CodeGenerationOptions options)
			: base(directory, options)
		{
		}

		protected override ServiceModel ProcessPregeneration(ServiceModel serviceModel)
		{
			return serviceModel;
		}

		protected override void GenerateClass(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
		}

		protected override void GenerateGateway(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".js"))
			{
				var classFileExpression = GatewayExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new JavascriptCodeGenerator(writer);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateEnum(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
		}
	}
}
