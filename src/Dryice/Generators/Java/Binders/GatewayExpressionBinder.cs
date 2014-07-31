using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java.Binders
{
	public class GatewayExpressionBinder
		: ServiceExpressionVisitor
	{
		private HashSet<Type> currentReturnTypes;
		private TypeDefinitionExpression currentTypeDefinitionExpression;
		public CodeGenerationContext CodeGenerationContext { get; set; }

		private DryType webServiceClientType;

		private GatewayExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext;

			webServiceClientType = DryType.Define(this.CodeGenerationContext.Options.ServiceClientTypeName ?? "WebServiceClient");
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
			var responseType = JavaBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);
			var responseTypeArgument = Expression.Variable(typeof(String), responseType.Name + ".class");
			var callback = Expression.Parameter(new DryType("RequestCallback<" + responseType.Name + ">"), "callback");

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
					throw new Exception("Post or Put method defined with null Content. You must define a @content field in your DryFile");
				}

				requestParameters = requestParameters.Where(x => x != contentParam).ToList();

				var payloadVar = Expression.Variable(typeof(string), "requestPayload");

				methodVariables.Add(payloadVar);

				var payloadAssign = Expression.Assign(payloadVar, DryExpression.StaticCall(contentParam.Type, typeof(String), "serialize", contentParam));

				methodStatements.Add(payloadAssign);

				serviceCallArguments = new
				{
					url,
					responseTypeArgument,
					payloadVar,
					callback
				};
			}
			else
			{
				serviceCallArguments = new
				{
					url,
					responseTypeArgument,
					callback
				};
			}

			foreach (var parameter in requestParameters)
			{
				var param = (ParameterExpression)parameter;

				if (param.Type is DryNullable)
				{
					param = DryExpression.Parameter(param.Type.GetUnwrappedNullableType(), param.Name);
				}

				var valueToReplace = Expression.Constant("{" + param.Name + "}", typeof(String));
				var valueAsString = DryExpression.Call(param, param.Type, typeof(String), SourceCodeGenerator.ToStringMethod, parameter);

				var replaceArgs = new
				{
					valueToReplace,
					valueAsString
				};

				methodStatements.Add(Expression.Assign(url, DryExpression.Call(url, typeof(String), "replace", replaceArgs)));
			}

			methodStatements.Add(DryExpression.Call(client, httpMethod, serviceCallArguments));

			var methodBody = DryExpression.Block
			(
				methodVariables.ToArray(),
				methodStatements.ToArray()
			);

			return new MethodDefinitionExpression(methodName, methodParameters.ToReadOnlyCollection(), typeof(void), methodBody, false, null);
		}

		private Expression CreateDefaultConstructor()
		{
			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var valParam = Expression.New(webServiceClientType);

			var body = DryExpression.Block(Expression.Assign(client, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, new Expression[] { }.ToReadOnlyCollection(), null, body, false, null, null);
		}

		private Expression CreateParameterisedConstructor()
		{
			var valParam = Expression.Parameter(webServiceClientType, "client");

			var parameters = new Expression[]
			{
				valParam
			};

			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var body = DryExpression.Block(Expression.Assign(client, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), null, body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentTypeDefinitionExpression = expression;
			currentReturnTypes = new HashSet<Type>(ReturnTypesCollector.CollectReturnTypes(expression));

			var includeExpressions = new List<IncludeExpression>
			{
				DryExpression.Include(expression.Type.Name),
				DryExpression.Include(this.CodeGenerationContext.Options.ResponseStatusTypeName),
				DryExpression.Include("com.jaigo.androiddevkit.RequestCallback"),
				DryExpression.Include("com.jaigo.androiddevkit.utils.ConvertUtils"),
				DryExpression.Include("com.jaigo.androiddevkit.WebServiceClient")
			};

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

			var singleValueResponseTypes = currentReturnTypes.Where(c => c.GetUnwrappedNullableType().IsPrimitive).Select(c => DryType.Define(JavaBinderHelpers.GetValueResponseWrapperTypeName(c))).ToList();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body).Append(singleValueResponseTypes).Distinct().ToList();
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			foreach (var referencedType in referencedTypes.Where(c => c is DryType && ((DryType)c).ServiceClass != null))
			{
				includeExpressions.Add(DryExpression.Include(referencedType.Name));
			}

			var headerGroup = includeExpressions.Sorted(IncludeExpression.Compare).ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false);
		}
	}
}
