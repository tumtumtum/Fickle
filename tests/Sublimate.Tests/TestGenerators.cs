using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Sublimate.Tests
{
	[TestFixture]
	public class TestGenerators
	{
		[Test]
		public void Test_GenerateObjcTypes_FromWebs()
		{
			var serviceModel = TestWebsParser.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out, CodeGenerationOptions.Default);

			serviceModelcodeGenerator.Generate(serviceModel);
		}
	}
}
