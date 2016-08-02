using System.IO;

namespace Fickle.Generators.CSharp
{
	public class SerializerInterfaceWriter
		: BraceLanguageStyleSourceCodeGenerator
	{
		private readonly CodeGenerationOptions codeGenerationOptions;

		public SerializerInterfaceWriter(TextWriter writer, CodeGenerationOptions codeGenerationOptions)
			: base(writer)
		{
			this.codeGenerationOptions = codeGenerationOptions;
		}

		public virtual void Write(ServiceModelInfo serviceModelInfo)
		{
			this.WriteLine("using System.IO;");
			this.WriteLine();
			this.WriteLine($"namespace {this.codeGenerationOptions.Namespace}");

			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
			{
				this.WriteLine("public interface IHttpStreamSerializer");

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					this.WriteLine("T Deserialize<T>(Stream inputStream);");
					this.WriteLine();
					this.WriteLine("string Serialize<T>(T value);");
				}
			}
		}
	}
}
