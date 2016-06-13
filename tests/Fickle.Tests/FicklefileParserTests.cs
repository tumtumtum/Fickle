using System;
using System.IO;
using System.Reflection;
using Fickle.Ficklefile;
using NUnit.Framework;
using Fickle.Model;

namespace Fickle.Tests
{
	[TestFixture]
	public class FicklefileParserTests
	{
		internal static ServiceModel GetTestServiceModel()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(FicklefileParserTests).Namespace + ".TestFiles.Test.fickle";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return FicklefileParser.Parse(reader);
				}
			}
		}

		internal static ServiceModel GetTestServiceModel2()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(FicklefileParserTests).Namespace + ".TestFiles.Test2.fickle";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return FicklefileParser.Parse(reader);
				}
			}
		}

		[Test]
		public void Test_Parse_And_Generate_ObjectiveC()
		{
			var serviceModel = FicklefileParserTests.GetTestServiceModel();

			var codeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", TextWriter.Null, CodeGenerationOptions.Default);

			codeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Parse_And_Generate_Javascript()
		{
			var serviceModel = FicklefileParserTests.GetTestServiceModel();
			var codeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("javascript", Console.Out, CodeGenerationOptions.Default);

			codeGenerator.Generate(serviceModel);
		}
	}
}
