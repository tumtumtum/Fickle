using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Sublimate.Generators.Objective
{
	public class ObjectiveLanguage
	{
		public static readonly SublimateType NSZoneType = new SublimateType("NSZone");
		public static readonly SublimateType NSMutableArray = new SublimateType("NSMutableArray");
		public static readonly SublimateType NSDictionary = new SublimateType("NSDictionary");
		
		public static ConstructorInfo MakeConstructorInfo(Type declaringType, string initMethodName, params object[] args)
		{
			var parameterInfos = new List<ParameterInfo>();

			for (var i = 0; i < args.Length; i += 2)
			{
				parameterInfos.Add(new SublimateParameterInfo((Type)args[i], (string)args[i + 1]));
			}

			return new SublimateConstructorInfo(declaringType, initMethodName, parameterInfos.ToArray());
		}

		public static MethodCallExpression MakeCall(Expression target, Type returnType, string methodName, Expression arg)
		{
			return Expression.Call(target, new SublimateMethodInfo(target.Type, returnType, methodName, new ParameterInfo[] { new SublimateParameterInfo(arg.Type, "arg0") }), arg);
		}
	}
}
