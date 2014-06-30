using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Dryice.Generators.Objective
{
	public class ObjectiveLanguage
	{
		public static readonly DryiceType NSZoneType = new DryiceType("NSZone");
		public static readonly DryiceType NSMutableArray = new DryiceType("NSMutableArray");
		public static readonly DryiceType NSDictionary = new DryiceType("NSDictionary");
		
		public static ConstructorInfo MakeConstructorInfo(Type declaringType, string initMethodName, params object[] args)
		{
			var parameterInfos = new List<ParameterInfo>();

			for (var i = 0; i < args.Length; i += 2)
			{
				parameterInfos.Add(new DryParameterInfo((Type)args[i], (string)args[i + 1]));
			}

			return new DryConstructorInfo(declaringType, initMethodName, parameterInfos.ToArray());
		}

		public static MethodCallExpression MakeCall(Expression target, Type returnType, string methodName, Expression arg)
		{
			return Expression.Call(target, new DryMethodInfo(target.Type, returnType, methodName, new ParameterInfo[] { new DryParameterInfo(arg.Type, "arg0") }), arg);
		}
	}
}
