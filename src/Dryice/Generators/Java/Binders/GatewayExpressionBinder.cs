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
		private Type currentType;
		private HashSet<Type> currentReturnTypes; 
		private TypeDefinitionExpression currentTypeDefinitionExpression;
		public CodeGenerationContext CodeGenerationContext { get; set; }

		private static readonly Regex urlParameterRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);

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
			
			var url = Expression.Variable(typeof(string), "url");

			var responseType = JavaBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);

			var variables = new [] { url };

			var hostname = currentTypeDefinitionExpression.Attributes["Hostname"];
			var path = "http://" + hostname + method.Attributes["Path"];

			var newParameters = new List<Expression>(method.Parameters);
			var callback = Expression.Parameter(new DryType("RequestCallback<" + responseType.Name + ">"), "callback");
			newParameters.Add(callback);

			var statements = new List<Expression>();

			statements.Add(Expression.Assign(url, Expression.Constant(path)));

			foreach (var parameter in method.Parameters)
			{
				var param = (ParameterExpression) parameter;
				var valueToReplace = Expression.Constant("{" + param.Name + "}", typeof (String));
				var valueAsString = DryExpression.Call(param, typeof(String), "toString", parameter);

				var replaceArgs = new
				{
					valueToReplace,
					valueAsString
				};

				statements.Add(Expression.Assign(url, DryExpression.Call(url, typeof(String), "replace", replaceArgs)));	
			}

			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var responseTypeArgument = Expression.Variable(typeof (String), responseType.Name + ".class"); 

			var serviceCallArguments = new
				{
					url, 
					responseTypeArgument,
					callback
				};

			var callMethod = method.Attributes["Method"];

			statements.Add(DryExpression.Call(client, callMethod, serviceCallArguments));


			var block = DryExpression.Block
			(
				variables,
				statements.ToArray()
			);

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), block, false, null);
		}

		private Expression CreateDefaultConstructor()
		{
			var client = Expression.Variable(webServiceClientType, "webServiceClient");

			var valParam = Expression.New(webServiceClientType);

			var body = DryExpression.Block(Expression.Assign(client, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, new Expression [] {}.ToReadOnlyCollection(), null, body, false, null, null, true);
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

			return new MethodDefinitionExpression(currentTypeDefinitionExpression.Type.Name, parameters.ToReadOnlyCollection(), null, body, false, null, null, true);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentType = expression.Type;
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

			currentType = null;

			return new TypeDefinitionExpression(expression.Type, header, body, false, null);
		}
	}
}
