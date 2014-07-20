using System;
using System.IO;
using System.Reflection;
using Dryice.Dryfile;
using NUnit.Framework;
using Dryice.Model;

namespace Dryice.Tests
{
	[TestFixture]
	public class DryFileParserTests
	{
		internal static ServiceModel GetTestServiceModel()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(DryFileParserTests).Namespace + ".TestFiles.Test.dryfile";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return DryfileParser.Parse(reader);
				}
			}
		}

		[Test]
		public void Test_Parse_And_Generate_ObjectiveC()
		{
			var serviceModel = DryFileParserTests.GetTestServiceModel();
			var codeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out, CodeGenerationOptions.Default);

			codeGenerator.Generate(serviceModel);
		}
	}
}
