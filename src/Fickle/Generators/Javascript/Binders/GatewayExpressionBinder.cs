using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Javascript.Binders
{
	public class GatewayExpressionBinder
		: ServiceExpressionVisitor
	{
		public CodeGenerationContext CodeGenerationContext { get; set; }
		private readonly FickleType webServiceClientType;
		private TypeDefinitionExpression currentTypeDefinitionExpression;

		private GatewayExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext;

			this.webServiceClientType = FickleType.Define(this.CodeGenerationContext.Options.ServiceClientTypeName ?? "WebServiceClient");
		}

		public static Expression Bind(CodeGenerationContext codeCodeGenerationContext, Expression expression)
		{
			var binder = new GatewayExpressionBinder(codeCodeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();
			var methodParameters = new List<Expression>(method.Parameters);
			var methodVariables = new List<ParameterExpression>();
			var methodStatements = new List<Expression>();

			var requestParameters = new List<Expression>(method.Parameters);

			var httpMethod = method.Attributes["Method"];
			var hostname = currentTypeDefinitionExpression.Attributes["Hostname"];
			var path = "http://" + hostname + method.Attributes["Path"];

			var client = Expression.Variable(webServiceClientType, "webServiceClient");
			var callback = Expression.Parameter(typeof(object), "onComplete");

			methodParameters.Add(callback);

			var url = Expression.Variable(typeof(string), "url");

			methodVariables.Add(url);
			methodStatements.Add(Expression.Assign(url, Expression.Constant(path)));

			Object serviceCallArguments;

			if (httpMethod.Equals("post", StringComparison.InvariantCultureIgnoreCase)
				|| httpMethod.Equals("put", StringComparison.InvariantCultureIgnoreCase))
			{
				var contentParameterName = method.Attributes["Content"];

				var contentParam = requestParameters.FirstOrDefault(x => ((ParameterExpression)x).Name.Equals(contentParameterName, StringComparison.InvariantCultureIgnoreCase));

				if (contentParam == null)
				{
					throw new Exception("Post or Put method defined with null Content. You must define a @content field in your FicklefileKeyword");
				}

				requestParameters = requestParameters.Where(x => x != contentParam).ToList();

				var payloadVar = Expression.Variable(typeof(string), "requestPayload");

				methodVariables.Add(payloadVar);

				var jsonBuilder = FickleType.Define("DefaultJsonBuilder");

				var jsonBuilderInstance = FickleExpression.StaticCall(jsonBuilder, "instance");

				var toJsonCall = FickleExpression.Call(jsonBuilderInstance, typeof(String), "toJson", contentParam);

				var payloadAssign = Expression.Assign(payloadVar, toJsonCall);

				methodStatements.Add(payloadAssign);

				serviceCallArguments = new
				{
					url,
					payloadVar,
					callback
				};
			}
			else
			{
				serviceCallArguments = new
				{
					url,
					callback
				};
			}

			foreach (var parameter in requestParameters)
			{
				var param = (ParameterExpression)parameter;

				if (param.Type is FickleNullable)
				{
					param = FickleExpression.Parameter(param.Type.GetUnwrappedNullableType(), param.Name);
				}

				var valueToReplace = Expression.Constant("{" + param.Name + "}", typeof(String));
				var valueAsString = FickleExpression.Call(param, param.Type, typeof(String), SourceCodeGenerator.ToStringMethod, parameter);

				var replaceArgs = new
				{
					valueToReplace,
					valueAsString
				};

				methodStatements.Add(Expression.Assign(url, FickleExpression.Call(url, typeof(String), "replace", replaceArgs)));
			}

			methodStatements.Add(FickleExpression.Call(client, httpMethod, serviceCallArguments));

			var methodBody = FickleExpression.Block
			(
				methodVariables.ToArray(),
				methodStatements.ToArray()
			);

			return new MethodDefinitionExpression(methodName, methodParameters.ToReadOnlyCollection(), AccessModifiers.Public, typeof(void), methodBody, false, null);
		}

		private Expression CreateDefaultConstructor()
		{
			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var valParam = Expression.New(webServiceClientType);

			var body = FickleExpression.Block(Expression.Assign(client, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, new Expression[] { }.ToReadOnlyCollection(), AccessModifiers.Public, null, body, false, null, null);
		}

		private Expression CreateParameterisedConstructor()
		{
			var valParam = Expression.Parameter(webServiceClientType, "client");

			var parameters = new Expression[]
			{
				valParam
			};

			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var body = FickleExpression.Block(Expression.Assign(client, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), AccessModifiers.Public, null, body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentTypeDefinitionExpression = expression;

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var client = new FieldDefinitionExpression("webServiceClient", webServiceClientType, AccessModifiers.Private | AccessModifiers.Constant);

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				client,
				CreateDefaultConstructor(),
				CreateParameterisedConstructor(),
				this.Visit(expression.Body)
			);

			var header = new Expression[] { comment }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false, expression.Attributes, expression.InterfaceTypes);
		}
	}
}
