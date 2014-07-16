using System;
using System.Diagnostics;
using NUnit.Framework;
using Platform.VirtualFileSystem;

namespace Dryice.Tests
{
	[TestFixture]
	public class GeneratorTests
	{
		[Test]
		public void Test_Generate_Objective_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = true
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = DryFileParserTests.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Objective_To_Console()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false
			};

			var serviceModel = DryFileParserTests.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", Console.Out, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}
	}
}
