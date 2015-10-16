using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Fickle.Expressions;

namespace Fickle.Generators
{
	/// <summary>
	/// Normalizes all user-defined names that conflict with keywords by prefixing them with a prefix string
	/// </summary>
	public class KeywordNormalizer
		: ServiceExpressionVisitor
	{
		private readonly string replacementPrefix;
		private readonly Func<string, string> normalize;
		private readonly HashSet<string> reservedKeywords;

		private KeywordNormalizer(string  replacementPrefix, IEnumerable<string> reservedKeywords, Func<string, string> normalize)
		{
			this.replacementPrefix = replacementPrefix;
			this.normalize = normalize;
			this.reservedKeywords = new HashSet<string>(reservedKeywords);
		}

		public static Expression Normalize(Expression expression, string replacementPrefix, IEnumerable<string> reservedKeywords, Func<string, string> normalize)
		{
			return new KeywordNormalizer(replacementPrefix, reservedKeywords, normalize).Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = this.normalize(property.PropertyName);
			var prefix = reservedKeywords.Contains(name) ? replacementPrefix : "";

			if (name != property.PropertyName || prefix != "")
			{
				return new PropertyDefinitionExpression(prefix + name, property.PropertyType, property.IsPredeclatation);
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
				var name = this.normalize(methodInfo.Name);
				var prefix = reservedKeywords.Contains(name) ? replacementPrefix : "";

                if (name != methodInfo.Name || prefix != "")
				{
					methodInfo = new FickleMethodInfo(methodInfo.DeclaringType, methodInfo.ReturnType, prefix + name, methodInfo.GetParameters(), methodInfo.IsStatic);

					return methodInfo;
				}
			}
			else if (memberInfo is FicklePropertyInfo)
			{
				var propertyInfo = (FicklePropertyInfo)memberInfo;
				var name = this.normalize(propertyInfo.Name);
				var prefix = reservedKeywords.Contains(name) ? replacementPrefix : "";

				if (name != propertyInfo.Name || prefix != "")
				{
					propertyInfo = new FicklePropertyInfo(propertyInfo.DeclaringType, propertyInfo.PropertyType, prefix + name);

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
			var name = this.normalize(node.Name);
			var prefix = reservedKeywords.Contains(name) ? replacementPrefix : "";

			if (name != node.Name || prefix != "")
			{
				return Expression.Parameter(node.Type, prefix + node.Name);
			}
			else
			{
				return base.VisitParameter(node);
			}
		}
	}
}
