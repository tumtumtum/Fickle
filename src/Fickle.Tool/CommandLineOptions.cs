using CommandLine;

namespace Fickle.Tool
{
	internal class CommandLineOptions
	{
		[Option('i', "input")]
		public string Input { get; set; }

		[Option('o', "outputdir")]
		public string Output { get; set; }

		[Option('l', "lang")]
		public string Language { get; set; }

		[Option('n', "name")]
		public string Name { get; set; }

		[Option('s', "summary")]
		public string Summary { get; set; }

		[Option('a', "author")]
		public string Author { get; set; }

		[Option('v', "version")]
		public string Version { get; set; }

		[Option("podspecsource")]
		public string PodspecSource { get; set; }
	}
}
