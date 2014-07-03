using System;
using NUnit.Framework;

namespace Dryice.Tests
{
	[TestFixture]
	public class TestGenerators
	{
		[Test]
		public void Test_GenerateObjcTypes_From_DryFile()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false
			};
			
			var serviceModel = TestWebsParser.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}
	}
}
