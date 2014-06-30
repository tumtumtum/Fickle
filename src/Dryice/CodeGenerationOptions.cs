using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dryice
{
	public class CodeGenerationOptions
	{
		public static readonly CodeGenerationOptions Default = new CodeGenerationOptions();

		public string BaseTypeName { get; set; }
	}
}
