﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dryice.Expressions
{
	[Flags]
	public enum AccessModifiers
	{
		None = 0x00,
		Public = 0x01,
		Private = 0x02,
		Protected = 0x04,
		Static = 0x08,
		Constant = 0x10
	}
}
