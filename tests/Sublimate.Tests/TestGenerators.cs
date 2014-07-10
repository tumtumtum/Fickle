﻿using System;
using NUnit.Framework;
using Platform.VirtualFileSystem;

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

		[Test]
		public void Test_Generate_Objective_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./Test_Generate_Objective_Files");
			var serviceModel = TestWebsParser.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}
	}
}
