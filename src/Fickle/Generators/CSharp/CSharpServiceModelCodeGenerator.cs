using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Fickle.Expressions;
using Fickle.Generators.CSharp.Binders;
using Platform.VirtualFileSystem;
using Fickle.Model;

namespace Fickle.Generators.CSharp
{
	[ServiceModelCodeGenerator("csharp")]
	public class CSharpServiceModelCodeGenerator
		: ServiceModelCodeGenerator
	{
		private Dictionary<string, Type> mappedTypes = new Dictionary<string, Type>();
		 
		public CSharpServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: base(file, options)
		{
		}

		public CSharpServiceModelCodeGenerator(TextWriter writer, CodeGenerationOptions options)
			: base(writer, options)
		{
		}

		public CSharpServiceModelCodeGenerator(IDirectory directory, CodeGenerationOptions options)
			: base(directory, options)
		{
		}

		protected override ServiceModel ProcessPregeneration(ServiceModel serviceModel)
		{
			return serviceModel;
		}

		public override void Generate(ServiceModel serviceModel)
		{
			this.GenerateHttpStreamSerializerInterface(serviceModel);

			foreach (var assemblyFile in this.Options.MappedTypeAssemblies)
			{
				var dllFile = new FileInfo(assemblyFile);
				var assembly = Assembly.LoadFile(dllFile.FullName);

				foreach (var mappedType in assembly.GetTypes())
				{
					this.mappedTypes[mappedType.Name] = mappedType;
				}
			}

			base.Generate(serviceModel);
		}

		protected virtual void GenerateHttpStreamSerializerInterface(ServiceModel serviceModel)
		{
			using (var writer = this.GetTextWriterForFile("IHttpStreamSerializer.cs"))
			{
				var serializerInterfaceWriter = new SerializerInterfaceWriter(writer, this.Options);

				serializerInterfaceWriter.Write(this.Options.ServiceModelInfo);
			}
		}

		protected override void GenerateClass(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			if (this.mappedTypes.ContainsKey(expression.Type.Name))
			{
				return;
			}

			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".cs"))
			{
				var classFileExpression = CSharpClassExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new CSharpCodeGenerator(writer, this.mappedTypes);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateGateway(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".cs"))
			{
				var classFileExpression = CSharpGatewayExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new CSharpCodeGenerator(writer, this.mappedTypes);

				codeGenerator.Generate(classFileExpression);
			}
		}

		protected override void GenerateEnum(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression)
		{
			if (this.mappedTypes.ContainsKey(expression.Type.Name))
			{
				return;
			}

			using (var writer = this.GetTextWriterForFile(expression.Type.Name + ".cs"))
			{
				var enumFileExpression = CSharpEnumExpressionBinder.Bind(codeGenerationContext, expression);

				var codeGenerator = new CSharpCodeGenerator(writer, this.mappedTypes);

				codeGenerator.Generate(enumFileExpression);
			}
		}
	}
}
