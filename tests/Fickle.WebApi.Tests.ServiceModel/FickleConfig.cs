using Fickle.WebApi;
using Fickle.WebApi.Tests.ServiceModel.ServiceModel;

[assembly: FickleIncludeType(typeof(IntBasedEnum), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(ByteBasedEnum), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(ShortBasedEnum), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(LongBasedEnum), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(EnumWithNegativeValues), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(EnumWithDuplicateValues), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(EnumWithFlags), IncludeRelatives = false)]
[assembly: FickleIncludeType(typeof(EnumWithKeywords), IncludeRelatives = false)] // TODO this doesn't parse
[assembly: FickleIncludeType(typeof(EnumWithNoValues), IncludeRelatives = false)]
