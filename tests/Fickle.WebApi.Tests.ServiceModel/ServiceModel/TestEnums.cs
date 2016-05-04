using System;

namespace Fickle.WebApi.Tests.ServiceModel.ServiceModel
{
	public enum IntBasedEnum : int
	{
		Min = int.MinValue,
		Max = int.MaxValue
	}

	public enum ByteBasedEnum : byte
	{
		Min = byte.MinValue,
		Max = byte.MaxValue
	}

	public enum ShortBasedEnum : short
	{
		Min = short.MinValue,
		Max = short.MaxValue
	}

	public enum LongBasedEnum : long
	{
		Min = long.MinValue,
		Max = long.MaxValue
	}

	public enum EnumWithNegativeValues
	{
		Negative = -1,
		Zero = 0,
		Positive = 1
	}

	public enum EnumWithDuplicateValues
	{
		Zero = 0,
		None = 0,
		One = 1,
		Uno = 1
	}

	[Flags]
	public enum EnumWithFlags
	{
		None = 0,
		Foo = 1,
		Bar = 2,
		Baz = 4,
		FooBar = Foo | Bar,
		All = Foo | Bar | Baz
	}

	public enum EnumWithKeywords
	{
		NonKeyword = 0,
		Info = 1,
		Class = 2,
		Gateway = 3,
		Enum = 4
	}

	public enum EnumWithNoValues
	{
	}
}