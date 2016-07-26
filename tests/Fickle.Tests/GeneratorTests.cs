using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Platform.VirtualFileSystem;

namespace Fickle.Tests
{
	[TestFixture]
	public class GeneratorTests
	{
		[Test]
		public void Test_Generate_Objective_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = true,
				TypeNamePrefix = "TN",
				SerializeEnumsAsStrings = false
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = FicklefileParserTests.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Objective_Files2()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = true,
				TypeNamePrefix = "TN",
				SerializeEnumsAsStrings = false
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = FicklefileParserTests.GetTestServiceModel2();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Objective_To_Console()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false,
				TypeNamePrefix = "TN"
			};

			var serviceModel = FicklefileParserTests.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("objc", TextWriter.Null, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Java_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = true,
				TypeNamePrefix = "TN",
				Namespace = "io.fickle.test.servicemodel"
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = FicklefileParserTests.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("java", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Java_To_Console()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false,
				TypeNamePrefix = "TN",
				Namespace = "io.fickle.test.servicemodel"
			};

			var serviceModel = FicklefileParserTests.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("java", TextWriter.Null, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Javascript_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false,
				TypeNamePrefix = "TN"
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = FicklefileParserTests.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("javascript", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_Javascript_To_Console()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = false,
				TypeNamePrefix = "TN"
			};

			var serviceModel = FicklefileParserTests.GetTestServiceModel();
			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("javascript", TextWriter.Null, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}

		[Test]
		public void Test_Generate_CSharp_Files()
		{
			var options = new CodeGenerationOptions
			{
				GenerateClasses = true,
				TypeNamePrefix = "TN",
				Namespace = "Io.Fickle.Test.Servicemodel"
			};

			var outputDir = FileSystemManager.Default.ResolveDirectory("./" + new StackTrace().GetFrame(0).GetMethod().Name);
			var serviceModel = FicklefileParserTests.GetTestServiceModel();

			outputDir.Create(true);

			var serviceModelcodeGenerator = ServiceModelCodeGenerator.GetCodeGenerator("csharp", outputDir, options);

			serviceModelcodeGenerator.Generate(serviceModel);
		}
	}
}
