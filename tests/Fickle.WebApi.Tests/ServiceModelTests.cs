using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Fickle.Ficklefile;
using Fickle.Reflectors;
using NUnit.Framework;

namespace Fickle.WebApi.Tests
{
	[TestFixture]
    public class ServiceModelTests
    {
		[Test]
		public void TestGenerateFickleFile()
		{
			var serviceModel = ReflectServiceModel();

			var writer = new FicklefileWriter(Console.Out);
			writer.Write(serviceModel);
		}

		[Test]
		public void TestGenerateFickleFileAndParse()
		{
			var reflectedServiceModel = ReflectServiceModel();

			using (var memoryStream = new MemoryStream())
			{
				using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
				{
					var writer = new FicklefileWriter(streamWriter);
					writer.Write(reflectedServiceModel);
				}

				memoryStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(memoryStream))
				{
					var fickleFile = streamReader.ReadToEnd();

					Console.WriteLine(fickleFile);

					var parsedServiceModel = FicklefileParser.Parse(fickleFile);

					AssertModelsMatch(parsedServiceModel, reflectedServiceModel);
				}
			}
		}

		private static Model.ServiceModel ReflectServiceModel()
		{
			var options = new ServiceModelReflectionOptions();

			var configuration = new HttpConfiguration();

			configuration.Routes.MapHttpRoute
				(
					name: "DefaultApi",
					routeTemplate: "api/{controller}/{action}"
				);

			configuration.EnsureInitialized();

			var assembly = Assembly.Load("Fickle.WebApi.Tests.ServiceModel");

			var reflector = new WebApiRuntimeServiceModelReflector(options, configuration, assembly, "localhost");
			return reflector.Reflect();
		}

		private static void AssertModelsMatch(Model.ServiceModel actual, Model.ServiceModel expected)
		{
			// ServiceModelInfo
			Assert.That(actual.ServiceModelInfo.Name, Is.EqualTo(expected.ServiceModelInfo.Name));
			Assert.That(actual.ServiceModelInfo.Author, Is.EqualTo(expected.ServiceModelInfo.Author));
			Assert.That(actual.ServiceModelInfo.ExtendedValues, Is.EquivalentTo(expected.ServiceModelInfo.ExtendedValues));
			Assert.That(actual.ServiceModelInfo.Homepage, Is.EqualTo(expected.ServiceModelInfo.Homepage));
			Assert.That(actual.ServiceModelInfo.License, Is.EqualTo(expected.ServiceModelInfo.License));
			Assert.That(actual.ServiceModelInfo.Summary, Is.EqualTo(expected.ServiceModelInfo.Summary));
			Assert.That(actual.ServiceModelInfo.Version, Is.EqualTo(expected.ServiceModelInfo.Version));

			// Enums
			Assert.That(actual.Enums.Count, Is.EqualTo(expected.Enums.Count));

			for (var i = 0; i < actual.Enums.Count; i++)
			{
				Assert.That(actual.Enums[i].Name, Is.EqualTo(expected.Enums[i].Name));

				Assert.That(actual.Enums[i].Values.Count, Is.EqualTo(expected.Enums[i].Values.Count));

				for (var j = 0; j < actual.Enums[i].Values.Count; j++)
				{
					Assert.That(actual.Enums[i].Values[j].Name, Is.EqualTo(expected.Enums[i].Values[j].Name));
					Assert.That(actual.Enums[i].Values[j].Value, Is.EqualTo(expected.Enums[i].Values[j].Value));
				}
			}

			// Classes
			Assert.That(actual.Classes.Count, Is.EqualTo(expected.Classes.Count));

			for (var i = 0; i < actual.Classes.Count; i++)
			{
				Assert.That(actual.Classes[i].Name, Is.EqualTo(expected.Classes[i].Name));
				Assert.That(actual.Classes[i].BaseTypeName, Is.EqualTo(expected.Classes[i].BaseTypeName));

				Assert.That(actual.Classes[i].Properties.Count, Is.EqualTo(expected.Classes[i].Properties.Count));

				for (var j = 0; j < actual.Classes[i].Properties.Count; j++)
				{
					Assert.That(actual.Classes[i].Properties[j].Name, Is.EqualTo(expected.Classes[i].Properties[j].Name));
					Assert.That(actual.Classes[i].Properties[j].TypeName, Is.EqualTo(expected.Classes[i].Properties[j].TypeName));
				}
			}

			// Gateways
			Assert.That(actual.Gateways.Count, Is.EqualTo(expected.Gateways.Count));

			for (var i = 0; i < actual.Gateways.Count; i++)
			{
				Assert.That(actual.Gateways[i].Name, Is.EqualTo(expected.Gateways[i].Name));
				Assert.That(actual.Gateways[i].BaseTypeName, Is.EqualTo(expected.Gateways[i].BaseTypeName));
				Assert.That(actual.Gateways[i].Hostname, Is.EqualTo(expected.Gateways[i].Hostname));

				Assert.That(actual.Gateways[i].Methods.Count, Is.EqualTo(expected.Gateways[i].Methods.Count));

				for (var j = 0; j < actual.Gateways[i].Methods.Count; j++)
				{
					Assert.That(actual.Gateways[i].Methods[j].Name, Is.EqualTo(expected.Gateways[i].Methods[j].Name));
					Assert.That(actual.Gateways[i].Methods[j].Authenticated, Is.EqualTo(expected.Gateways[i].Methods[j].Authenticated));
					Assert.That(actual.Gateways[i].Methods[j].Content, Is.EqualTo(expected.Gateways[i].Methods[j].Content));
					Assert.That(actual.Gateways[i].Methods[j].ContentFormat, Is.EqualTo(expected.Gateways[i].Methods[j].ContentFormat));
					Assert.That(actual.Gateways[i].Methods[j].ContentServiceParameter?.Name, Is.EqualTo(expected.Gateways[i].Methods[j].ContentServiceParameter?.Name));
					Assert.That(actual.Gateways[i].Methods[j].ContentServiceParameter?.TypeName, Is.EqualTo(expected.Gateways[i].Methods[j].ContentServiceParameter?.TypeName));
					Assert.That(actual.Gateways[i].Methods[j].Method, Is.EqualTo(expected.Gateways[i].Methods[j].Method));
					Assert.That(actual.Gateways[i].Methods[j].Path, Is.EqualTo(expected.Gateways[i].Methods[j].Path));
					Assert.That(actual.Gateways[i].Methods[j].RequestFormat, Is.EqualTo(expected.Gateways[i].Methods[j].RequestFormat));
					Assert.That(actual.Gateways[i].Methods[j].ReturnFormat, Is.EqualTo(expected.Gateways[i].Methods[j].ReturnFormat));
					Assert.That(actual.Gateways[i].Methods[j].Returns, Is.EqualTo(expected.Gateways[i].Methods[j].Returns));
					Assert.That(actual.Gateways[i].Methods[j].Secure, Is.EqualTo(expected.Gateways[i].Methods[j].Secure));

					Assert.That(actual.Gateways[i].Methods[j].Parameters.Count, Is.EqualTo(expected.Gateways[i].Methods[j].Parameters.Count));

					for (var k = 0; k < actual.Gateways[i].Methods[j].Parameters.Count; k++)
					{
						Assert.That(actual.Gateways[i].Methods[j].Parameters[k].Name, Is.EqualTo(expected.Gateways[i].Methods[j].Parameters[k].Name));
						Assert.That(actual.Gateways[i].Methods[j].Parameters[k].TypeName, Is.EqualTo(expected.Gateways[i].Methods[j].Parameters[k].TypeName));
					}
				}
			}
		}
	}
}
