using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
		private readonly HashSet<string> reservedKeywords;

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
				return new PropertyDefinitionExpression(replacementPrefix + property.PropertyName, property.PropertyType, property.IsPredeclatation);
			}
			else
			{
				return base.VisitPropertyDefinitionExpression(property);
			}
		}

		protected MemberInfo Normalize(MemberInfo memberInfo)
		{
			if (memberInfo is FickleMethodInfo)
			{
				var methodInfo = (FickleMethodInfo)memberInfo;

				if (reservedKeywords.Contains(methodInfo.Name))
				{
					methodInfo = new FickleMethodInfo(methodInfo.DeclaringType, methodInfo.ReturnType, replacementPrefix + methodInfo.Name, methodInfo.GetParameters(), methodInfo.IsStatic);

					return methodInfo;
				}
			}
			else if (memberInfo is FicklePropertyInfo)
			{
				var propertyInfo = (FicklePropertyInfo)memberInfo;

				if (reservedKeywords.Contains(propertyInfo.Name))
				{
					propertyInfo = new FicklePropertyInfo(propertyInfo.DeclaringType, propertyInfo.PropertyType, replacementPrefix + propertyInfo.Name);

					return propertyInfo;
				}
			}

			return memberInfo;
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
		{
			var replacement = this.Normalize(node.Member);

			if (replacement != node.Member)
			{
				return Expression.Bind(replacement, this.Visit(node.Expression));
			}
			else
			{
				return base.VisitMemberAssignment(node);
			}
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.NodeType == ExpressionType.MemberAccess)
			{
				var replacement = this.Normalize(node.Member);

				if (replacement != node.Member)
				{
					if (node.Member is FickleMethodInfo)
					{
						return Expression.MakeMemberAccess(this.Visit(node.Expression), replacement);
					}
					else if (node.Member is FicklePropertyInfo)
					{
						return Expression.MakeMemberAccess(this.Visit(node.Expression), replacement);
					}
				}
			}

			return base.VisitMember(node);
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
