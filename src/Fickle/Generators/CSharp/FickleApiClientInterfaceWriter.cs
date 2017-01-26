using System.IO;

namespace Fickle.Generators.CSharp
{
	public class FickleApiClientInterfaceWriter
		: BraceLanguageStyleSourceCodeGenerator
	{
		private readonly CodeGenerationOptions codeGenerationOptions;

		public FickleApiClientInterfaceWriter(TextWriter writer, CodeGenerationOptions codeGenerationOptions)
			: base(writer)
		{
			this.codeGenerationOptions = codeGenerationOptions;
		}

		public virtual void Write(ServiceModelInfo serviceModelInfo)
		{
			this.WriteLine("using System.Threading.Tasks;");
			this.WriteLine();
			this.WriteLine($"namespace {this.codeGenerationOptions.Namespace}");

			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
			{
				this.WriteLine("public interface IFickleApiClient");

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					this.WriteLine("Task<TResult> ExecuteAsync<TResult>(string requestUrl, string method, bool isSecure, bool isAuthenticated, string returnFormat);");
					this.WriteLine();
					this.WriteLine("Task<TResult> ExecuteAsync<TResult, TContent>(string requestUrl, string method, bool isSecure, bool isAuthenticated, string returnFormat, TContent requestContent);");
					this.WriteLine();
					this.WriteLine("Task ExecuteAsync(string requestUrl, string method, bool isSecure, bool isAuthenticated, string returnFormat);");
					this.WriteLine();
					this.WriteLine("Task ExecuteAsync<TContent>(string requestUrl, string method, bool isSecure, bool isAuthenticated, string returnFormat, TContent requestContent);");
				}
			}
		}
	}
}
