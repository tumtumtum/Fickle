using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform.Reflection;
using Platform.VirtualFileSystem;
using Dryice.Model;

namespace Dryice
{
	public abstract class ServiceModelCodeGenerator
		: IDisposable
	{
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
		
		protected ServiceModelCodeGenerator(IFile file)
			: this(file.GetContent().GetWriter())
		{
		}

		protected ServiceModelCodeGenerator(TextWriter writer)
		{
			this.writer = new TextWriterWrapper(writer);

			if (writer == Console.Out)
			{
				this.writer.NewLine = "\n";
			}
		}

		protected ServiceModelCodeGenerator(IDirectory directory)
		{
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

		public abstract void Generate(ServiceModel serviceModel);

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
