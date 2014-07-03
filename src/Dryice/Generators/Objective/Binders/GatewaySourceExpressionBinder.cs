using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Objective.Binders
{
	public class GatewaySourceExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly ServiceModel serviceModel;
		private readonly CodeGenerationOptions options;

		private GatewaySourceExpressionBinder(ServiceModel serviceModel, CodeGenerationOptions options)
		{
			this.options = options; 
			this.serviceModel = serviceModel;
		}

		public static Expression Bind(ServiceModel serviceModel, Expression expression, CodeGenerationOptions options)
		{
			var binder = new GatewaySourceExpressionBinder(serviceModel, options);

			return binder.Visit(expression);
		}

		private static readonly Regex urlParameterRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();
			var serviceMethodInfo = (ServiceMethodDefinitionExpression)method;

			var relativeUri = Expression.Variable(typeof(string), "relativeUri");

			var variables = new ParameterExpression[]
			{
				relativeUri,
				Expression.Variable(new DryType(this.options.ServiceClientTypeName ??"PKWebServiceClient"), "client")
			};

			var url = serviceMethodInfo.ServiceMethod.Url;
			var names = new List<string>();
			var parameters = new List<ParameterDefinitionExpression>();
			var parametersByName = serviceMethodInfo.Parameters.ToDictionary(c => ((ParameterDefinitionExpression)c).ParameterName, c => (ParameterDefinitionExpression)c, StringComparer.InvariantCultureIgnoreCase);

			var objcUrl = urlParameterRegex.Replace(url, delegate(Match match)
			{
				var name = match.Groups[1].Value;
				
				names.Add(name);

				var parameter = parametersByName[name];

				parameters.Add(parameter);

				return "%@";
			});

			// [NSString stringWithFormat:@"", ...] methodinfo
			var parameterInfos = new List<DryParameterInfo>();
			parameterInfos.Add(new ObjectiveParameterInfo(typeof(string), "s"));
			parameterInfos.AddRange(parameters.Select(c => new ObjectiveParameterInfo(typeof(string), c.ParameterName, true)));
			var methodInfo = new DryMethodInfo(typeof(string), typeof(string), "stringWithFormat", parameterInfos.ToArray(), true);

			var args = new List<Expression>();

			args.Add(Expression.Constant(objcUrl));
			args.AddRange(parameters.Select(c => Expression.Call(Expression.Parameter(c.ParameterType, c.ParameterName), typeof(object).GetMethod("ToString", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))));

			var body = new Expression[]
			{
				Expression.Assign(relativeUri, Expression.Call(null, methodInfo, args).ToStatement()),
				Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement()
			}.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new MethodDefinitionExpression(methodName, method.Parameters, method.ReturnType, Expression.Block(variables, body), false, null);
		}
			
		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			var includeExpressions = new List<Expression>
			{
				new IncludeStatementExpression(expression.Name + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var body = GroupedExpressionsExpression.FlatConcat(GroupedExpressionsExpressionStyle.Wide, this.Visit(expression.Body));

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, false, null);
		}
	}
}
