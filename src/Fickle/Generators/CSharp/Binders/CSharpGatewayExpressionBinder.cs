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

		private readonly FickleType fickleApiClientType;

		public const string FickleApiClientFieldName = "fickleApiClient";
		public const string HostnameFieldName = "Hostname";

		private CSharpGatewayExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext;

			this.fickleApiClientType = FickleType.Define("IFickleApiClient");
		}

		public static Expression Bind(CodeGenerationContext codeCodeGenerationContext, Expression expression)
		{
			var binder = new CSharpGatewayExpressionBinder(codeCodeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var client = Expression.Variable(this.fickleApiClientType, FickleApiClientFieldName);

			var apiCallGenericTypes = new List<Type>();

			var returnTaskType = FickleType.Define("Task");

			if (method.ReturnType != typeof (void))
			{
				returnTaskType.MakeGenericType(method.ReturnType);
			}

			var methodParameters = new List<Expression>(method.Parameters);
			var methodVariables = new List<ParameterExpression>();
			var methodStatements = new List<Expression>();

			var requestUrl = Expression.Variable(typeof(InterpolatedString), "fickleRequestUrl");
			methodVariables.Add(requestUrl);
			methodStatements.Add(Expression.Assign(requestUrl, Expression.Constant(new InterpolatedString(method.Attributes["Path"]))));

			var httpMethod = Expression.Constant(method.Attributes["Method"]);
			var isSecure = Expression.Constant(method.Attributes["Secure"] == "True");
			var isAuthenticated = Expression.Constant(method.Attributes["Authenticated"] == "True");
			var returnFormat = Expression.Constant(method.Attributes["ReturnFormat"]);

			

			if (method.ReturnType != typeof (void))
			{
				apiCallGenericTypes.Add(method.ReturnType);
			}

			object apiArgs;

			var contentParameterName = method.Attributes["Content"];

			if (!string.IsNullOrEmpty(contentParameterName))
			{
				var contentParam = method.Parameters.FirstOrDefault(x => ((ParameterExpression) x).Name.Equals(contentParameterName, StringComparison.InvariantCultureIgnoreCase));

				if (contentParam == null)
				{
					throw new Exception("Content paramter not found");
				}

				apiCallGenericTypes.Add(contentParam.Type);

				apiArgs = new
				{
					requestUrl,
					httpMethod,
					isSecure,
					isAuthenticated,
					returnFormat,
					contentParam
				};
			}
			else
			{
				apiArgs = new
				{
					requestUrl,
					httpMethod,
					isSecure,
					isAuthenticated,
					returnFormat
				};
			}

			var clientCall = FickleExpression.Call(client, returnTaskType, "ExecuteAsync", apiArgs);

			if (apiCallGenericTypes.Count > 0)
			{
				clientCall.Method.MakeGenericMethod(apiCallGenericTypes.ToArray());
			}
			
			var result = Expression.Variable(returnTaskType, "fickleResult");
			methodVariables.Add(result);

			methodStatements.Add(Expression.Assign(result, clientCall));
			methodStatements.Add(FickleExpression.Return(result));

			var methodBody = FickleExpression.Block
			(
				methodVariables.ToArray(),
				methodStatements.ToArray()
			);

			return new MethodDefinitionExpression(method.Name, methodParameters.ToReadOnlyCollection(), AccessModifiers.Public, returnTaskType, methodBody, false, null);
		}

		private Expression CreateParameterisedConstructor()
		{
			var fickleApiClientParam = Expression.Parameter(this.fickleApiClientType, FickleApiClientFieldName);

			var parameters = new Expression[]
			{
				fickleApiClientParam
			};

			var clientField = Expression.Variable(this.fickleApiClientType, "this." + FickleApiClientFieldName);

			var body = FickleExpression.Block(Expression.Assign(clientField, fickleApiClientParam).ToStatement());

			return new MethodDefinitionExpression(this.currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), AccessModifiers.Public, null, body, false, null, null);
		}

		private Expression CreateStaticConstructor()
		{
			var hostname = this.currentTypeDefinitionExpression.Attributes["Hostname"];
			var hostnameField = Expression.Variable(typeof(string), HostnameFieldName);
			var body = FickleExpression.Block(Expression.Assign(hostnameField, Expression.Constant(hostname)).ToStatement());

			var parameters = new Expression[0];

			return new MethodDefinitionExpression(this.currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), AccessModifiers.Static, null, body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			this.currentTypeDefinitionExpression = expression;

			var includeExpressions = new List<IncludeExpression>
			{
				FickleExpression.Include("System"),
				FickleExpression.Include("System.Collections.Generic"),
				FickleExpression.Include("System.Threading.Tasks")
			};

			foreach (var include in this.CodeGenerationContext.Options.Includes)
			{
				includeExpressions.Add(FickleExpression.Include(include));
			}

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var hostname = new FieldDefinitionExpression(HostnameFieldName, typeof(string), AccessModifiers.Public | AccessModifiers.Static | AccessModifiers.ReadOnly);
			var client = new FieldDefinitionExpression(FickleApiClientFieldName, this.fickleApiClientType, AccessModifiers.Private | AccessModifiers.ReadOnly);

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				hostname,
				client,
				this.CreateStaticConstructor(),
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
