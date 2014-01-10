using System;
using System.Collections.Generic;
using System.Linq;
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
			var webs = @"
				enum Sex
					Male : 1
					Female : 2
					Both

				class ResponseStatus
					Message : string

				class User
					Id : uuid
					Birthdate : datetime
					Name : string
					Password : string
					Age : int?
					Sex : Sex
					TimeSinceLastLogin : timespan
					Friends : [User]
					FollowerUserIds : [uuid]
			";

			var serviceModel =  WebsParser.Parse(webs);

			Console.WriteLine(XmlSerializer<ServiceModel>.New().SerializeToString(serviceModel));
		}
	}
}
