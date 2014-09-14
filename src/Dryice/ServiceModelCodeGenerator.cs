using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Fickle.Dryfile;
using Fickle.Expressions;
using Fickle.Generators;
using Platform.IO;
using Platform.Reflection;
using Platform.VirtualFileSystem;
using Fickle.Model;

namespace Fickle
{
	public abstract class ServiceModelCodeGenerator
		: IDisposable
	{
		public CodeGenerationOptions Options { get; set; }

		#region TextWriterWrapper
		private class TextWriterWrapper
			: TextWriter
		{
			private readonly TextWriter inner;

			public TextWriterWrapper(TextWriter inner)
			{
				this.inner = inner;
				this.NewLine = inner.NewLine;
			}

			public override void Write(char value)
			{
				this.inner.Write(value);
			}

			public override Encoding Encoding
			{
				get
				{
					return this.inner.Encoding;
				}
			}

			public void DisposeInner()
			{
				this.inner.Dispose();
			}
		}
		#endregion

		private TextWriterWrapper writer;
		private readonly IDirectory directory;

		public static ServiceModelCodeGenerator GetCodeGenerator(string language, IDirectory directory, CodeGenerationOptions options)
		{
			return ServiceModelCodeGenerator.GetCodeGenerator(language, (object)directory, options);
		}

		public static ServiceModelCodeGenerator GetCodeGenerator(string language, IFile file, CodeGenerationOptions options)
		{
			return ServiceModelCodeGenerator.GetCodeGenerator(language, (object)file, options);
		}

		public static ServiceModelCodeGenerator GetCodeGenerator(string language, TextWriter writer, CodeGenerationOptions options)
		{
			return ServiceModelCodeGenerator.GetCodeGenerator(language, (object)writer, options);
		}

		public static ServiceModelCodeGenerator GetCodeGenerator(string language, object param, CodeGenerationOptions options)
		{
			var types = typeof(ServiceModelCodeGenerator).Assembly.GetTypes();
			var serviceModelCodeGeneratorTypes = types.Where(c => typeof(ServiceModelCodeGenerator).IsAssignableFrom(c));
			var generatorType = serviceModelCodeGeneratorTypes.FirstOrDefault(delegate(Type type)
			{
				if (Regex.Match(type.Name, language + "ServiceModelCodeGenerator$", RegexOptions.IgnoreCase).Success)
				{
					return true;
				}

				var attribute = type.GetFirstCustomAttribute<ServiceModelCodeGeneratorAttribute>(true);

				if (attribute != null && attribute.Aliases.FirstOrDefault(c => c.Equals(language, StringComparison.InvariantCultureIgnoreCase)) != null)
				{	
					return true;
				}

				return false;
			});

			if (generatorType != null)
			{
				return (ServiceModelCodeGenerator)Activator.CreateInstance(generatorType, new [] { param, options });
			}

			return null;
		}
		
		protected ServiceModelCodeGenerator(IFile file, CodeGenerationOptions options)
			: this(file.GetContent().GetWriter(), options)
		{
		}

		protected ServiceModelCodeGenerator(TextWriter writer, CodeGenerationOptions options)
		{
			this.Options = options;
			this.writer = new TextWriterWrapper(writer);

			if (writer == Console.Out)
			{
				this.writer.NewLine = "\n";
			}
		}

		protected ServiceModelCodeGenerator(IDirectory directory, CodeGenerationOptions options)
		{
			this.Options = options;
			this.directory = directory;

			directory.Create(true);
		}

		private int getWriterCount;

		protected TextWriter GetTextWriterForFile(string fileName)
		{
			getWriterCount++;

			if (this.writer != null)
			{
				if (getWriterCount > 1)
				{
					this.writer.WriteLine();
				}

				return this.writer;
			}

			if (this.directory != null)
			{
				return this.directory.ResolveFile(fileName).GetContent().GetWriter();
			}

			return null;
		}

		protected virtual ServiceModel ProcessPregeneration(ServiceModel serviceModel)
		{
			return serviceModel;
		}

		public virtual void Generate(ServiceModel serviceModel)
		{
			serviceModel = this.ProcessPregeneration(serviceModel);

			var codeGenerationContext = new CodeGenerationContext(serviceModel, this.Options);
			var serviceExpressionBuilder = new ServiceExpressionBuilder(serviceModel, this.Options);

			if (this.Options.GenerateClasses)
			{
				foreach (var expression in serviceModel.Classes.Select(serviceExpressionBuilder.Build).Cast<TypeDefinitionExpression>())
				{
					var currentExpression = expression;

					this.GenerateClass(codeGenerationContext, currentExpression);
				}
			}

			if (this.Options.GenerateGateways)
			{
				foreach (var expression in serviceModel.Gateways.Select(serviceExpressionBuilder.Build).Cast<TypeDefinitionExpression>())
				{
					var currentExpression = expression;

					this.GenerateGateway(codeGenerationContext, currentExpression);
				}
			}

			if (this.Options.GenerateEnums)
			{
				foreach (var expression in serviceModel.Enums.Select(serviceExpressionBuilder.Build).Cast<TypeDefinitionExpression>())
				{
					var currentExpression = expression;

					this.GenerateEnum(codeGenerationContext, currentExpression);
				}
			}

			var assembly = Assembly.GetExecutingAssembly();
			var prefix = this.GetType().Namespace + ".Prelude.";

			foreach (var resourceName in assembly.GetManifestResourceNames().Where(resourceName => resourceName.StartsWith(prefix)))
			{
				using (var input = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
				{
					using (var output = this.GetTextWriterForFile(resourceName.Substring(prefix.Length)))
					{
						int x;

						while ((x = input.Read()) != -1)
						{
							output.Write((char)x);
						}
					}
				}
			}
		}

		protected abstract void GenerateEnum(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression);
		protected abstract void GenerateClass(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression);
		protected abstract void GenerateGateway(CodeGenerationContext codeGenerationContext, TypeDefinitionExpression expression);

		public virtual void Dispose()
		{
			if (this.writer != null)
			{
				this.writer.DisposeInner();
				this.writer = null;
			}
		}
	}
}
