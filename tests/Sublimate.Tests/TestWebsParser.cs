using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Platform.Xml.Serialization;
using Dryice.Model;
using Dryice.Webs;

namespace Dryice.Tests
{
	[TestFixture]
	public class TestWebsParser
	{
		public static ServiceModel GetTestServiceModel()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(TestWebsParser).Namespace + ".TestFiles.Test.dry";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return WebsParser.Parse(reader);
				}
			}
		}

		public void Test_Parse()
		{
			var serviceModel = TestWebsParser.GetTestServiceModel();

			Console.WriteLine(XmlSerializer<ServiceModel>.New().SerializeToString(serviceModel));
		}

		[Test]
		public void Test_Parse_And_Generator_ObjectiveC()
		{
			var serviceModel = TestWebsParser.GetTestServiceModel();
			var codeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out, CodeGenerationOptions.Default);

			codeGenerator.Generate(serviceModel);
		}
	}
}
