using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Platform.Xml.Serialization;
using Sublimate.Generators.Objective;
using Sublimate.Model;
using Sublimate.Webs;

namespace Sublimate.Tests
{
	[TestFixture]
	public class TestWebsParser
	{
		private ServiceModel GetTestServiceModel()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = this.GetType().Namespace + ".TestFiles.Test.webs";

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
			var serviceModel = this.GetTestServiceModel();

			Console.WriteLine(XmlSerializer<ServiceModel>.New().SerializeToString(serviceModel));
		}

		[Test]
		public void Test_Parse_And_Generator_ObjectiveC()
		{
			var serviceModel = this.GetTestServiceModel();
			var codeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out);

			codeGenerator.Generate(serviceModel);
		}
	}
}
