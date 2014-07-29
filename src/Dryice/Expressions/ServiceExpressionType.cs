namespace Dryice.Expressions
{
	public enum ServiceExpressionType
	{
		GroupedExpressions = 0x1000,
		Statement = GroupedExpressions + 1,
		MethodDefinition = GroupedExpressions + 2,
		PropertyDefinition = GroupedExpressions + 4,
		TypeDefinition = GroupedExpressions + 5,
		IncludeStatement = GroupedExpressions + 6,
		ReferencedType = GroupedExpressions + 7,
		Comment = GroupedExpressions + 8,
		ForEach = GroupedExpressions + 9,
		CodeLiteral = GroupedExpressions + 10,
		SimpleLambdaExpression = GroupedExpressions + 11,
		FieldDefinition = GroupedExpressions + 12
	}
}
