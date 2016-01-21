using CommandLine;

namespace Fickle.Tool
{
	internal class CommandLineOptions
	{
		[OptionArray('i', "input", HelpText = "The input files")]
		public string[] Input { get; set; }

		[Option('o', "outputdir")]
		public string Output { get; set; }

		[Option('l', "lang")]
		public string Language { get; set; }

		[Option('n', "name")]
		public string Name { get; set; }

		[Option('h', "homepage")]
		public string Homepage { get; set; }

		[Option('m', "license")]
		public string License { get; set; }

		[Option('s', "summary")]
		public string Summary { get; set; }

		[Option('a', "author")]
		public string Author { get; set; }

		[Option('v', "version")]
		public string Version { get; set; }

		[Option('p', "pod", DefaultValue = false)]
		public bool Pod { get; set; }

		[Option('f', "framework", DefaultValue = false)]
		public bool ImportDependenciesAsFramework { get; set; }

		[Option("podspecsource")]
		public string PodspecSource { get; set; }

		[Option("podspecsourcefiles")]
		public string PodspecSourceFiles { get; set; }
	}
}
