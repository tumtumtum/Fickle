using CommandLine;

namespace Dryice.Tool
{
	internal class CommandLineOptions
	{
		[Option('i', "input")]
		public string InputFile { get; set; }

		[Option('n', "name")]
		public string Name { get; set; }
		
		[Option('s', "summary")]
		public string Summary { get; set; }

		[Option('a', "author")]
		public string Author { get; set; }

		[Option("podspec-platforms")]
		public string PodspecPlatforms { get; set; }
	}
}
