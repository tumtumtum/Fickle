using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
			base.Generate(serviceModel);

			if (this.Options.GeneratePod)
			{
				this.GeneratePodspec(serviceModel);
			}

			this.GenerateMasterHeader(serviceModel);
		}

		protected virtual void GenerateMasterHeader(ServiceModel serviceModel)
		{
			using (var writer = this.GetTextWriterForFile(this.Options.ServiceModelInfo.Name + ".h"))
			{
				var headerWriter = new ObjectiveCodeGenerator(writer);

				var includeExpressions = serviceModel.Classes
					.Select(c => FickleExpression.Include(c.Name + ".h"))
					.Concat(serviceModel.Enums.Select(c => FickleExpression.Include(c.Name + ".h")))
					.Concat(serviceModel.Gateways.Select(c => FickleExpression.Include(c.Name + ".h")));

				var comment = new CommentExpression("This file is AUTO GENERATED");

				var commentGroup = new[] { comment }.ToStatementisedGroupedExpression();
				var headerGroup = includeExpressions.ToStatementisedGroupedExpression();

				var headerExpressions = new List<Expression>
				{
					commentGroup,
					headerGroup
				};

				headerWriter.Visit(headerExpressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide));
			}
        }

		protected override bool IncludePreludeResource(string path)
		{
			if (this.Options.GeneratePod)
			{
				return !path.StartsWith("PlatformKit.");
			}
			return base.IncludePreludeResource(path);
		}

		protected virtual void GeneratePodspec(ServiceModel serviceModel)
		{
			using (var writer = this.GetTextWriterForFile(this.Options.ServiceModelInfo.Name + ".podspec"))
			{
				var podspecWriter = new PodspecWriter(writer);

				podspecWriter.Write(this.Options.ServiceModelInfo);
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
			List<Expression> methods;

			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".m"))
			{
				var classFileExpression = GatewaySourceExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				methods = ExpressionGatherer.Gather(classFileExpression, ServiceExpressionType.MethodDefinition);

				codeGenerator.Generate(classFileExpression);
			}
			
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".h"))
			{
				var headerFileExpression = GatewayHeaderExpressionBinder.Bind(codeGenerationContext, expression, methods.Cast<MethodDefinitionExpression>().ToList());

				var codeGenerator = new ObjectiveCodeGenerator(writer);

				codeGenerator.Generate(headerFileExpression);
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
