using System.Collections.Generic;
using CommandLine;

namespace Dryice.Tool
{
	internal class CommandLineOptions
	{
		[Option("i")]
		public string InputDryFile { get; set; }
		
		[OptionList('r', "referenceassemblies", Separator = ':', HelpText = "Specify the paths of the assemblies to reflect")]
		public List<string> ReferenceAssemblies { get; set; }
	}
}
