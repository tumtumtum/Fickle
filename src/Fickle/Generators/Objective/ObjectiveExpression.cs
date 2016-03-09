using System;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.Objective
{
    public static class ObjectiveExpression
    {
        public static Expression ToPercentEscapeEncodeExpression(Expression value)
        {
            var cfstring = FickleType.Define("CFStringRef", isPrimitive: true);
            var charsToEncode = Expression.Convert(Expression.Constant("!*'\\\"();:@&= +$,/?%#[]% "), cfstring);

            var retval = Expression.Convert(FickleExpression.StaticCall((Type)null, cfstring, "CFURLCreateStringByAddingPercentEscapes", new { arg1 = (object)null, arg2 = Expression.Convert(value, cfstring), arg3 = (object)null, arg4 = charsToEncode, arg5 = Expression.Variable(typeof(int), "NSUTF8StringEncoding") }), typeof(string));

            return retval;
        }
    }
}