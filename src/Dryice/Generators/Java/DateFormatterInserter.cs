using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dryice.Expressions;

namespace Dryice.Generators.Java
{
	public class DateFormatterInserter
		: ServiceExpressionVisitor
	{
		private bool containsDateConversion = false;

		public static Expression Insert(Expression expression)
		{
			return new DateFormatterInserter().Visit(expression);
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
				var dateFormatter = Expression.Variable(new DryType("NSDateFormatter"), "dateFormatter");
				variables.Add(dateFormatter);
				var expressions = new List<Expression>();

				// dateFormatter = [[NSDateFormatter alloc]init]
				expressions.Add(Expression.Assign(dateFormatter, Expression.New(new DryType("NSDateFormatter"))).ToStatement());
				// [dateFormatter setTimeZone: [NSTimeZone timeZoneWithAbbreviation:@"UTC"]];
				expressions.Add(DryExpression.Call(dateFormatter, "setTimeZone", DryExpression.StaticCall("NSTimeZone", "NSTimeZone", "timeZoneWithAbbreviation", "UTC")).ToStatement());
				// [dateFormatter setDateFormat: @"yyyy-MM-ddTHH:mm:ss"];
				expressions.Add(DryExpression.Call(dateFormatter, "setDateFormat", "yyyy-MM-ddTHH:mm:ss").ToStatement());
				
				expressions.AddRange(block.Expressions);

				var newBody = Expression.Block(variables, expressions);

				return new MethodDefinitionExpression(retval.Name, retval.Parameters, retval.ReturnType, newBody, retval.IsPredeclaration, retval.RawAttributes);
			}

			return retval;
		}
	}
}
