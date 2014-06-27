namespace Sublimate.Expressions
{
	public enum ServiceExpressionType
	{
		GroupedExpressions = 0x1000,
		Statement = GroupedExpressions + 1,
		MethodDefinition = GroupedExpressions + 2,
		ParameterDefinition = GroupedExpressions + 3,
		PropertyDefinition = GroupedExpressions + 4,
		TypeDefinition = GroupedExpressions + 5,
		IncludeStatement = GroupedExpressions + 6,
		ReferencedType = GroupedExpressions + 7,
		Comment = GroupedExpressions + 8
	}
}
