using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Fickle.Expressions;

namespace Fickle.Generators.Objective
{
	public class DateFormatterInserter
		: ServiceExpressionVisitor
	{
		private bool containsDateConversion = false;

		public static Expression Insert(Expression expression)
		{
			return new DateFormatterInserter().Visit(expression);
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Convert && (node.Type == typeof(DateTime?) || node.Type == typeof(DateTime)))
			{
				this.containsDateConversion = true;
			}

			return base.VisitUnary(node);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.Name == "ToString" && node.Object != null && (node.Object.Type == typeof(DateTime) || node.Object.Type == typeof(DateTime?)))
			{
				this.containsDateConversion = true;
			}

			return base.VisitMethodCall(node);
		}

		protected override Expression VisitMethodDefinitionExpression(Expressions.MethodDefinitionExpression method)
		{
			this.containsDateConversion = false;

			var retval = (MethodDefinitionExpression)base.VisitMethodDefinitionExpression(method);

			if (this.containsDateConversion && retval.Body is BlockExpression)
			{
				var block = (BlockExpression)retval.Body;
				var variables = new List<ParameterExpression>(block.Variables);
				var dateFormatter = Expression.Variable(new FickleType("NSDateFormatter"), "dateFormatter");
				variables.Add(dateFormatter);
				var expressions = new List<Expression>();

				// dateFormatter = [[NSDateFormatter alloc]init]
				expressions.Add(Expression.Assign(dateFormatter, Expression.New(new FickleType("NSDateFormatter"))).ToStatement());
				// [dateFormatter setTimeZone: [NSTimeZone timeZoneWithAbbreviation:@"UTC"]];
				expressions.Add(FickleExpression.Call(dateFormatter, "setTimeZone", FickleExpression.StaticCall("NSTimeZone", "NSTimeZone", "timeZoneWithAbbreviation", "UTC")).ToStatement());
				// Javascript: yyyy-MM-dd'T'HH:mm:ss.SSSSz
				// ISO 8601: yyyy-MM-dd'T'HH:mm:ssZZZZZ
				expressions.Add(FickleExpression.Call(dateFormatter, "setDateFormat", "yyyy-MM-dd'T'HH:mm:ss.SSSSz").ToStatement());
				
				expressions.AddRange(block.Expressions);

				var newBody = Expression.Block(variables, expressions);

				return new MethodDefinitionExpression(retval.Name, retval.Parameters, retval.ReturnType, newBody, retval.IsPredeclaration, retval.RawAttributes);
			}

			return retval;
		}
	}
}
