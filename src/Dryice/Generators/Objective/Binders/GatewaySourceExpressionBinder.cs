using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private Type currentType;
		private TypeDefinitionExpression currentTypeDefinitionExpression;
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
			
			var self = Expression.Variable(currentType, "self");
			var optionsvar = DryExpression.Variable("NSMutableDictionary", "localOptions");
			var url = Expression.Variable(typeof(string), "url");
			var client = Expression.Variable(DryType.Make(this.options.ServiceClientTypeName ?? "PKWebServiceClient"), "client");
			var operationQueue = DryExpression.Property(self, DryType.Make("NSOperationQueue"), "operationQueue");

			var variables = new [] { url, client, optionsvar };

			var hostname = currentTypeDefinitionExpression.Attributes["Hostname"];
			var path = "http://" + hostname + method.Attributes["Path"];
			var names = new List<string>();
			var parameters = new List<ParameterExpression>();
			var parametersByName = method.Parameters.ToDictionary(c => ((ParameterExpression)c).Name, c => (ParameterExpression)c, StringComparer.InvariantCultureIgnoreCase);

			var objcUrl = urlParameterRegex.Replace(path, delegate(Match match)
			{
				var name = match.Groups[1].Value;
				
				names.Add(name);

				var parameter = parametersByName[name];

				parameters.Add(parameter);

				return "%@";
			});

			var parameterInfos = new List<DryParameterInfo>();
			parameterInfos.Add(new ObjectiveParameterInfo(typeof(string), "s"));
			parameterInfos.AddRange(parameters.Select(c => new ObjectiveParameterInfo(typeof(string), c.Name, true)));
			var methodInfo = new DryMethodInfo(typeof(string), typeof(string), "stringWithFormat", parameterInfos.ToArray(), true);

			var args = new List<Expression>();

			args.Add(Expression.Constant(objcUrl));
			args.AddRange(parameters.Select(c => Expression.Call(Expression.Parameter(c.Type, c.Name), typeof(object).GetMethod("ToString", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))));

			var block = DryExpression.Block
			(
				variables,
				Expression.Assign(optionsvar, DryExpression.MakeMethodCall(DryExpression.Property(self, DryType.Define("NSDictionary"), "options"), "NSMutableDictionary", "mutableCopyWithZone", new
				{
					zone = Expression.Constant(null, DryType.Define("NSZone"))
				})),
				DryExpression.MakeMethodCall(optionsvar, "setObject", new
				{
					obj = DryExpression.MakeStaticMethodCall(method.ReturnType, "class", null),
					forKey = "ResponseClass"
				}),
				Expression.Assign(url, Expression.Call(null, methodInfo, args)),
				Expression.Assign(client, DryExpression.MakeMethodCall(Expression.Variable(currentType, "self"), "PKWebServiceClient", "createClientWithURL", new
				{
					url,
					options = optionsvar,
					operationQueue
				})),
				Expression.Assign(DryExpression.Property(client, currentType, "delegate"), self),
				Expression.Return(Expression.Label(), DryExpression.MakeMethodCall(client, "getWithCallback", "test"))
			);

			return new MethodDefinitionExpression(methodName, method.Parameters, method.ReturnType, block, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateClientMethod()
		{
			var client = Expression.Variable(DryType.Define("PKWebServiceClient"), "client");
			var self = DryExpression.Variable(currentType, "self");
			var optionsvar = DryExpression.Parameter(DryType.Define("NSDictionary"), "optionsIn");
			var url = Expression.Parameter(typeof(string), "urlIn");
			var parameters = new Expression[] { url, optionsvar };
			var operationQueue = DryExpression.Property(self, DryType.Define("NSOperationQueue"), "operationQueue");

			var variables = new [] { client };

			var body = DryExpression.Block
			(
				variables,
				Expression.Assign(client, DryExpression.MakeStaticMethodCall("PKWebServiceClient", "PKWebServiceClient", "clientWithHost", new
				{
					url,
					options = optionsvar,
					operationQueue
				})),
				Expression.Return(Expression.Label(), client)
			);

			return new MethodDefinitionExpression("createClientWithURL", new ReadOnlyCollection<Expression>(parameters), DryType.Define("PKWebServiceClient"), body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateErrorResponseWithErrorCodeMethod()
		{
			var client = Expression.Parameter(DryType.Define("PKWebServiceClient"), "client");
			var errorCode = Expression.Parameter(typeof(string), "createErrorResponseWithErrorCode");
			var message = Expression.Parameter(typeof(string), "message");
			
			var parameters = new Expression[]
			{
				client,
				errorCode,
				message
			};

			var clientOptions = DryExpression.Property(client, DryType.Define("NSDictionary"), "options");

			var response = DryExpression.Variable(typeof(object), "response");
			var responseClass = DryExpression.MakeMethodCall(clientOptions, "Class", "objectForKey", "ResponseClass");
			var responseStatus = DryExpression.Property(response, DryType.Define("ResponseStatus"), "responseStatus");
			var newResponseStatus = DryExpression.New("ResponseStatus", "init", null);

			var body = DryExpression.Block
			(
				new [] { response },
				Expression.Assign(response, DryExpression.MakeMethodCall(DryExpression.MakeMethodCall(responseClass, response.Type, "alloc", null), response.Type, "init", null)),
				Expression.IfThen(Expression.IsTrue(Expression.Equal(responseStatus, Expression.Constant(null, responseStatus.Type))), DryExpression.Block(Expression.Assign(responseStatus, newResponseStatus))),
				Expression.Assign(DryExpression.Property(responseStatus, typeof(string), "errorCode"), errorCode),
				Expression.Assign(DryExpression.Property(responseStatus, typeof(string), "message"), message),
				Expression.Return(Expression.Label(), response)
			);

			return new MethodDefinitionExpression("webServiceClient", new ReadOnlyCollection<Expression>(parameters), DryType.Define("id"), body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateSerializeRequestMethod()
		{
			var client = Expression.Parameter(DryType.Define("PKWebServiceClient"), "client");
			var requestObject = Expression.Parameter(DryType.Define("id"), "serializeRequest");
			var dictionary = DryExpression.Variable("NSDictionary", "dictionary");
			var error = DryExpression.Variable("NSError", "error");
			var retval = DryExpression.Variable("NSData", "retval");

			var parameters = new Expression[]
			{
				client,
				requestObject
			};

			var allPropertiesAsDictionary = DryExpression.MakeMethodCall(requestObject, "NSDictionary", "allPropertiesAsDictionary", null);

			var serializedDataObject = DryExpression.MakeStaticMethodCall("NSJSONSerialization", "NSData", "dataWithJSONObject", new
			{
				obj = allPropertiesAsDictionary,
				options = Expression.Constant(0),
				error = DryExpression.Parameter(DryType.Define("NSError", true), "error")
			});

			var body = DryExpression.Block
			(
				new[] { dictionary, error, retval },
				Expression.Assign(retval, serializedDataObject),
				Expression.Return(Expression.Label(), retval)
			);

			return new MethodDefinitionExpression("webServiceClient", new ReadOnlyCollection<Expression>(parameters), retval.Type, body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateParseResultMethod()
		{
			var self = Expression.Variable(currentType, "self");
			var client = Expression.Parameter(DryType.Define("PKWebServiceClient"), "client");
			var dataObject = Expression.Parameter(DryType.Define("NSData"), "parseResult");
			var contentType = Expression.Parameter(typeof(string), "contentType");
			var statusCode = Expression.Parameter(typeof(int), "statusCode");
			var error = DryExpression.Variable("NSError", "error");
			var propertyDictionaryVar = DryExpression.Variable("NSDictionary", "propertyDictionary");

			var retval = DryExpression.Variable("id", "retval");

			var parameters = new Expression[]
			{
				client,
				dataObject,
				contentType,
				statusCode
			};

			var propertyDictionary = DryExpression.MakeStaticMethodCall("NSJSONSerialization", "NSData", "JSONObjectWithData", new
			{
				dataObject,
				options = Expression.Constant(0),
				error = DryExpression.Parameter(DryType.Define("NSError", true), "error")
			});

			var clientOptions = DryExpression.Property(client, DryType.Define("NSDictionary"), "options");

			var responseClass = DryExpression.MakeMethodCall(clientOptions, "Class", "objectForKey", "ResponseClass");
			var deserializedObject = DryExpression.MakeMethodCall(DryExpression.MakeMethodCall(responseClass, typeof(object), "alloc", null), typeof(object), "initWithProperty", new
			{
				propertyDictionaryVar
			});

			var parseErrorResult = DryExpression.MakeMethodCall(self, "webServiceClient", new
			{
				client,
				createErrorResponseWithErrorCode = "JsonDeserializationError",
				andMessage = DryExpression.MakeMethodCall(error, "localizedDescription", null)
			});

			var body = DryExpression.Block
			(
				new[] { retval, error },
				Expression.Assign(propertyDictionaryVar, propertyDictionary),
				Expression.IfThen(Expression.IsTrue(Expression.Equal(propertyDictionaryVar, Expression.Constant(null, propertyDictionaryVar.Type))), Expression.Return(Expression.Label(), parseErrorResult).ToStatement().ToBlock()),
				Expression.Assign(retval, deserializedObject),
				Expression.Return(Expression.Label(), retval)
			);

			return new MethodDefinitionExpression("webServiceClient", new ReadOnlyCollection<Expression>(parameters), retval.Type, body, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentType = expression.Type;
			currentTypeDefinitionExpression = expression;

			var includeExpressions = new List<Expression>
			{
				new IncludeStatementExpression(expression.Name + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide, 
				CreateCreateClientMethod(), 
				this.CreateCreateErrorResponseWithErrorCodeMethod(), 
				this.CreateSerializeRequestMethod(),
				this.CreateParseResultMethod(),
				this.Visit(expression.Body)
			);

			currentType = null;

			return new TypeDefinitionExpression(expression.Type, expression.BaseType, header, body, false, null);
		}
	}
}
