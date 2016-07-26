using System;

namespace Fickle.Expressions
{
	[Flags]
	public enum AccessModifiers
	{
		None = 0x00,
		Public = 0x01,
		Private = 0x02,
		Protected = 0x04,
		Static = 0x08,
		Constant = 0x10,
		ClasseslessFunction = 0x20,
		ReadOnly = 0x22
	}
}
