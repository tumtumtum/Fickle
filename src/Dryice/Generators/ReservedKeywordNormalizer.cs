using System.Collections.Generic;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators
{
	/// <summary>
	/// Normalizes all user-defined names that conflict with keywords by prefixing them with a prefix string
	/// </summary>
	public class ReservedKeywordNormalizer
		: ServiceExpressionVisitor
	{
		private readonly string replacementPrefix;
		private readonly HashSet<string> reservedKeywords = new HashSet<string>();

		private ReservedKeywordNormalizer(string  replacementPrefix, IEnumerable<string> reservedKeywords)
		{
			this.replacementPrefix = replacementPrefix;
			this.reservedKeywords = new HashSet<string>(reservedKeywords);
		}

		public static Expression Normalize(Expression expression, string replacementPrefix, IEnumerable<string> reservedKeywords)
		{
			return new ReservedKeywordNormalizer(replacementPrefix, reservedKeywords).Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			if (reservedKeywords.Contains(property.PropertyName))
			{
				return new PropertyDefinitionExpression(property.PropertyName, property.PropertyType, property.IsPredeclatation);
			}
			else
			{
				return base.VisitPropertyDefinitionExpression(property);
			}
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (reservedKeywords.Contains(node.Name))
			{
				return Expression.Parameter(node.Type, replacementPrefix + node.Name);
			}
			else
			{
				return base.VisitParameter(node);
			}
		}
	}
}
