using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Platform.Xml.Serialization;
using Sublimate.Model;
using Sublimate.Webs;

namespace Sublimate.Tests
{
	[TestFixture]
	public class TestWebsParser
	{
		[Test]
		public void TestParse()
		{
			string webs;
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = this.GetType().Namespace + ".TestFiles.Test.webs";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					webs = reader.ReadToEnd();
				}
			}

			var serviceModel =  WebsParser.Parse(webs);

			Console.WriteLine(XmlSerializer<ServiceModel>.New().SerializeToString(serviceModel));
		}
	}
}
