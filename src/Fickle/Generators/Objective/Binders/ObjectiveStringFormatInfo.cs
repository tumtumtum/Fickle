using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Fickle.Expressions;

namespace Fickle.Generators.Objective.Binders
{
	public struct ObjectiveStringFormatInfo
	{
		private static readonly Regex tempateStringRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
		
		public string Format { get; }
		public List<Expression> ValueExpressions { get; }
		public List<ParameterExpression> ParameterExpressions { get; }

		public ObjectiveStringFormatInfo(string format, List<Expression> valueExpressions, List<ParameterExpression> parameterExpressions)
		{
			this.Format = format;
			this.ValueExpressions = valueExpressions;
			this.ParameterExpressions = parameterExpressions;
		}

		public static ObjectiveStringFormatInfo GetObjectiveStringFormatInfo(string path, Func<string, Expression> valueByKey, Func<Expression, Expression> transformStringArg = null)
		{
			var args = new List<Expression>();
			var parameters = new List<ParameterExpression>();

			var format = tempateStringRegex.Replace(path, delegate (Match match)
			{
				var name = match.Groups[1].Value;

				var parameter = valueByKey(name);
				var type = parameter.Type;

				if (type == typeof(byte) || type == typeof(short) || type == typeof(int))
				{
					parameters.Add(Expression.Parameter(parameter.Type, name));
					args.Add(parameter);

					return "%d";
				}
				else if (type == typeof(long))
				{
					parameters.Add(Expression.Parameter(parameter.Type, name));
					args.Add(parameter);

					return "%lld";
				}
				else if (type == typeof(float) || type == typeof(double))
				{
					parameters.Add(Expression.Parameter(parameter.Type, name));
					args.Add(parameter);

					return "%f";
				}
				else if (type == typeof(char))
				{
					parameters.Add(Expression.Parameter(parameter.Type, name));
					args.Add(parameter);

					return "%C";
				}
				else if (type == typeof(int))
				{
					parameters.Add(Expression.Parameter(parameter.Type, name));
					args.Add(parameter);

					return "%d";
				}
				else if (type == typeof(Guid))
				{
					parameters.Add(Expression.Parameter(typeof(string), name));
					var arg = FickleExpression.Call(parameter, typeof(string), "ToString", null);

					args.Add(Expression.Condition(Expression.Equal(arg, Expression.Constant(null)), Expression.Constant(""), arg));

					return "%@";
				}
				else
				{
					parameters.Add(Expression.Parameter(typeof(string), name));
					var arg = (Expression)FickleExpression.Call(parameter, typeof(string), "ToString", null);

					arg = FickleExpression.Call(arg, typeof(string), "stringByAddingPercentEscapesUsingEncoding", Expression.Variable(typeof(int), "NSUTF8StringEncoding"));

					if (transformStringArg != null)
					{
						arg = transformStringArg(arg);
					}

					args.Add(Expression.Condition(Expression.Equal(arg, Expression.Constant(null)), Expression.Constant(""), arg));

					return "%@";
				}
			});

			return new ObjectiveStringFormatInfo(format, args, parameters);
		}
	}
}