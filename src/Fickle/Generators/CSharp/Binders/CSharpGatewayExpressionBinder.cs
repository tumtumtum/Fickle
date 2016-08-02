using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;


namespace Fickle.Generators.CSharp.Binders
{
	public class CSharpGatewayExpressionBinder
		: ServiceExpressionVisitor
	{
		private TypeDefinitionExpression currentTypeDefinitionExpression;
		public CodeGenerationContext CodeGenerationContext { get; set; }

		private readonly FickleType httpClientType;
		private readonly FickleType httpStreamSerializerType;

		public const string HttpClientFieldName = "httpClient";
		public const string HttpStreamSerializerFieldName = "httpStreamSerializer";

		private CSharpGatewayExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext;

			this.httpClientType = FickleType.Define(this.CodeGenerationContext.Options.ServiceClientTypeName ?? "HttpClient");
			this.httpStreamSerializerType = FickleType.Define("IHttpStreamSerializer");
		}

		public static Expression Bind(CodeGenerationContext codeCodeGenerationContext, Expression expression)
		{
			var binder = new CSharpGatewayExpressionBinder(codeCodeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var client = Expression.Variable(this.httpClientType, HttpClientFieldName);

			var methodName = method.Name;
			var methodParameters = new List<Expression>(method.Parameters);
			var methodVariables = new List<ParameterExpression>();
			var methodStatements = new List<Expression>();

			var hostname = this.currentTypeDefinitionExpression.Attributes["Hostname"];
			var fullPath = "http://" + hostname + method.Attributes["Path"];
			var relativePath = method.Attributes["Path"];

			if (relativePath.StartsWith("/"))
			{
				relativePath = relativePath.Substring(1);
			}

			var requestUrl = Expression.Variable(typeof(InterpolatedString), "requestUrl");
			methodVariables.Add(requestUrl);

			var baseAddressProperty = FickleExpression.Property(client, this.httpClientType, "BaseAddress");
			methodStatements.Add(Expression.IfThenElse(Expression.Equal(baseAddressProperty, Expression.Constant(null)),
				Expression.Assign(requestUrl, Expression.Constant(new InterpolatedString(fullPath))).ToStatementBlock(),
				Expression.Assign(requestUrl, Expression.Constant(new InterpolatedString(relativePath))).ToStatementBlock()));

			var httpMethodType = FickleType.Define("HttpMethod");
			var httpRequestMessageType = FickleType.Define("HttpRequestMessage");
			var httpRequestMessagesArgs = new
			{
				httpMethod = FickleExpression.New(httpMethodType, "HttpMethod", method.Attributes["Method"]),
				requestUrl
			};

			var httpRequestMessageNew = FickleExpression.New(httpRequestMessageType, "HttpRequestMessage", httpRequestMessagesArgs);
			var httpRequestMessage = Expression.Variable(httpRequestMessageType, "httpRequestMessage");
			methodVariables.Add(httpRequestMessage);
			methodStatements.Add(Expression.Assign(httpRequestMessage, httpRequestMessageNew));

			var streamType = FickleType.Define("Stream");
			var httpStreamSerializer = Expression.Variable(this.httpStreamSerializerType, HttpStreamSerializerFieldName);

			var contentParameterName = method.Attributes["Content"];

			if (!string.IsNullOrEmpty(contentParameterName))
			{
				var contentParam = method.Parameters.FirstOrDefault(x => ((ParameterExpression)x).Name.Equals(contentParameterName, StringComparison.InvariantCultureIgnoreCase));

				if (contentParam == null)
				{
					throw new Exception("Content paramter not found");
				}

				var serializeCall = FickleExpression.Call(httpStreamSerializer, typeof(string), "Serialize", contentParam);
				var stringContentNew = FickleExpression.New(FickleType.Define("StringContent"), "StringContent", serializeCall);
				methodStatements.Add(Expression.Assign(FickleExpression.Property(httpRequestMessage, httpRequestMessageType, "Content"), stringContentNew));
			}

			var httpResponseMessageType = FickleType.Define("HttpResponseMessage");
			var httpResponseMessage = Expression.Variable(httpResponseMessageType, "httpResponseMessage");
			methodVariables.Add(httpResponseMessage);

			var clientCall = FickleExpression.Call(client, new CSharpAwaitedTaskType(httpResponseMessageType), "SendAsync", httpRequestMessage);
			methodStatements.Add(Expression.Assign(httpResponseMessage, clientCall));
			methodStatements.Add(FickleExpression.Call(httpResponseMessage, "EnsureSuccessStatusCode", null));

			if (method.ReturnType != typeof (void))
			{
				var result = Expression.Variable(method.ReturnType, "result");
				methodVariables.Add(result);

				var contentStream = Expression.Variable(streamType, "contentStream");
				methodVariables.Add(contentStream);
				methodStatements.Add(Expression.Assign(contentStream, Expression.Constant(null)));

				var responseContent = Expression.Property(httpResponseMessage, "Content");
				var contentStreamCall = FickleExpression.Call(responseContent, new CSharpAwaitedTaskType(streamType), "ReadAsStreamAsync", null);

				var deserializeCall = FickleExpression.Call(httpStreamSerializer, method.ReturnType, "Deserialize", contentStream);
				deserializeCall.Method.MakeGenericMethod(method.ReturnType);

				var tryBlock = FickleExpression.Grouped(
					Expression.Assign(contentStream, contentStreamCall).ToStatement(),
					Expression.Assign(result, deserializeCall).ToStatement()
					);

				var diposeStream = Expression.IfThen(Expression.NotEqual(contentStream, Expression.Constant(null)), FickleExpression.Call(contentStream, "Dispose", null).ToStatementBlock());

				var tryFinally = Expression.TryFinally(tryBlock, diposeStream);
				methodStatements.Add(tryFinally);

				methodStatements.Add(FickleExpression.Return(result));
			}
			
			var methodBody = FickleExpression.Block
			(
				methodVariables.ToArray(),
				methodStatements.ToArray()
			);

			var returnType = method.ReturnType != typeof (void) ? method.ReturnType : null;
			var returnTaskType = new CSharpAwaitedTaskType(returnType);

			return new MethodDefinitionExpression(methodName, methodParameters.ToReadOnlyCollection(), AccessModifiers.Public, returnTaskType, methodBody, false, null);
		}

		private Expression CreateParameterisedConstructor()
		{
			var httpClientParam = Expression.Parameter(this.httpClientType, HttpClientFieldName);
			var httpStreamSerializerParam = Expression.Parameter(this.httpStreamSerializerType, HttpStreamSerializerFieldName);

			var parameters = new Expression[]
			{
				httpClientParam,
				httpStreamSerializerParam
			};

			var clientField = Expression.Variable(this.httpClientType, "this." + HttpClientFieldName);
			var serailizerField = Expression.Variable(this.httpStreamSerializerType, "this." + HttpStreamSerializerFieldName);

			var body = FickleExpression.Block(new Expression[]
			{
				Expression.Assign(clientField, httpClientParam).ToStatement(),
				Expression.Assign(serailizerField, httpStreamSerializerParam).ToStatement()

			} );

			return new MethodDefinitionExpression(this.currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), AccessModifiers.Public, null, body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			this.currentTypeDefinitionExpression = expression;

			var includeExpressions = new List<IncludeExpression>
			{
				FickleExpression.Include("System"),
				FickleExpression.Include("System.Collections.Generic"),
				FickleExpression.Include("System.Net.Http"),
				FickleExpression.Include("System.Threading.Tasks"),
				FickleExpression.Include("System.IO"),
				FickleExpression.Include("System.Net")
			};

			foreach (var include in this.CodeGenerationContext.Options.Includes)
			{
				includeExpressions.Add(FickleExpression.Include(include));
			}

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var client = new FieldDefinitionExpression(HttpClientFieldName, this.httpClientType, AccessModifiers.Private | AccessModifiers.ReadOnly);
			var serializer = new FieldDefinitionExpression(HttpStreamSerializerFieldName, this.httpStreamSerializerType, AccessModifiers.Private | AccessModifiers.ReadOnly);

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				client,
				serializer,
				this.CreateParameterisedConstructor(),
				this.Visit(expression.Body)
			);

			var headerGroup = includeExpressions.Sorted(IncludeExpression.Compare).ToGroupedExpression();

			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var typeDefinitionExpression = new TypeDefinitionExpression(expression.Type, null, body, false, expression.Attributes, expression.InterfaceTypes);

			return new NamespaceExpression(this.CodeGenerationContext.Options.Namespace, header, typeDefinitionExpression);
		}
	}
}
