using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
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
		private HashSet<Type> currentReturnTypes; 
		private TypeDefinitionExpression currentTypeDefinitionExpression;
		public CodeGenerationContext CodeGenerationContext { get; set; }
		
		private GatewaySourceExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.CodeGenerationContext = codeGenerationContext; 
		}

		public static Expression Bind(CodeGenerationContext codeCodeGenerationContext, Expression expression)
		{
			var binder = new GatewaySourceExpressionBinder(codeCodeGenerationContext);

			return binder.Visit(expression);
		}

		private MethodDefinitionExpression CreateInitWithOptionsMethod()
		{
			var self = DryExpression.Variable(currentType, "self");
			var super = DryExpression.Variable(currentType, "super");
			var options = DryExpression.Parameter("NSDictionary", "options");
			var superinit = DryExpression.Call(super, currentType, "init", null);

			var initBlock = DryExpression.Block(new Expression[]
			{
				Expression.Assign(DryExpression.Property(self, "NSDictionary", "options"), options)
			});

			var body = DryExpression.Block(new Expression[]
			{
				Expression.IfThen(Expression.NotEqual(Expression.Assign(self, superinit), Expression.Constant(null, currentType)), initBlock),
				Expression.Return(Expression.Label(), self)
			});

			return new MethodDefinitionExpression("initWithOptions", new Expression[] { options }.ToReadOnlyCollection(), DryType.Define("id"), body, false, null);
		}

		private static readonly Regex urlParameterRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);

		public static bool IsNumericType(Type type)
		{
			return type.IsIntegerType() || type.IsRealType();
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();
			
			var self = Expression.Variable(currentType, "self");
			var options = DryExpression.Variable("NSMutableDictionary", "localOptions");
			var url = Expression.Variable(typeof(string), "url");
			var client = Expression.Variable(DryType.Define(this.CodeGenerationContext.Options.ServiceClientTypeName ?? "PKWebServiceClient"), "client");
			var responseType = ObjectiveBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);

			var variables = new [] { url, client, options };

			var hostname = currentTypeDefinitionExpression.Attributes["Hostname"];
			var path = "http://" + hostname + method.Attributes["Path"];
			var httpMethod = method.Attributes["Method"];
			var names = new List<string>();
			var parameters = new List<ParameterExpression>();
			var args = new List<Expression>();
			
			var parametersByName = method.Parameters.ToDictionary(c => ((ParameterExpression)c).Name, c => (ParameterExpression)c, StringComparer.InvariantCultureIgnoreCase);

			var objcUrl = urlParameterRegex.Replace(path, delegate(Match match)
			{
				var name = match.Groups[1].Value;
				
				names.Add(name);

				var parameter = parametersByName[name];
				var type = parameter.Type;

				if (type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%d";
				}
				else if (type == typeof(char))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%C";
				}
				else if (type == typeof(int))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%d";
				}
				else if (type is DryListType)
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);
					
					return "%@";
				}
				else
				{
					parameters.Add(Expression.Parameter(typeof(string), parameter.Name));
					var arg = DryExpression.Call(parameter, typeof(string), "ToString", null);

					arg = DryExpression.Call(arg, typeof(string), "stringByAddingPercentEscapesUsingEncoding", Expression.Variable(typeof(int), "NSUTF8StringEncoding"));

					args.Add(arg);

					return "%@";
				}
			});

			var parameterInfos = new List<DryParameterInfo>();

			parameterInfos.Add(new ObjectiveParameterInfo(typeof(string), "s"));
			parameterInfos.AddRange(parameters.Select(c => new ObjectiveParameterInfo(c.Type, c.Name, true)));
			var methodInfo = new DryMethodInfo(typeof(string), typeof(string), "stringWithFormat", parameterInfos.ToArray(), true);

			args.Insert(0, Expression.Constant(objcUrl));
			
			var newParameters = new List<Expression>(method.Parameters);
			var callback = Expression.Parameter(new DryDelegateType(typeof(void), new DryParameterInfo(responseType, "response")), "callback");

			newParameters.Add(callback);

			Expression blockArg = Expression.Parameter(DryType.Define("id"), "arg1");

			var returnType = method.ReturnType;

			if (TypeSystem.IsPrimitiveType(method.ReturnType))
			{
				returnType = DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(method.ReturnType));
			}

			var conversion = Expression.Convert(blockArg, returnType);
			var body = DryExpression.Call(callback, "Invoke", conversion).ToStatement();
			var conversionBlock = DryExpression.SimpleLambda(body, blockArg);
			
			Expression clientCallExpression;

			if (httpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
			{
				clientCallExpression = DryExpression.Call(client, "getWithCallback", conversionBlock);
			}
			else
			{
				var contentParameterName = method.Attributes["Content"];
				var content = parametersByName[contentParameterName];

				clientCallExpression = DryExpression.Call(client, "postWithRequestObject", new
				{
					requestObject = content,
					andCallback = conversionBlock
				});
			}

			var block = DryExpression.Block
			(
				variables,
				Expression.Assign(callback, DryExpression.Call(callback, callback.Type, "copy", null)),
				Expression.Assign(options, DryExpression.Call(DryExpression.Property(self, DryType.Define("NSDictionary"), "options"), "NSMutableDictionary", "mutableCopyWithZone", new
				{
					zone = Expression.Constant(null, DryType.Define("NSZone"))
				})),
				DryExpression.Grouped
				(
					DryExpression.Call(options, "setObject", new
					{
						obj = responseType == typeof(void) ? (Expression)DryExpression.StaticCall(DryType.Define("NSNull"), DryType.Define("id"), "null", null) : (Expression)DryExpression.StaticCall(responseType, "class", null),
						forKey = "$ResponseClass"
					}).ToStatement(),
					method.ReturnType.GetUnwrappedNullableType() == typeof(bool) ?
					DryExpression.Call(options, "setObject", new
					{
						obj = Expression.Convert(Expression.Constant(1), typeof(object))
					}).ToStatement() : null
				),
				Expression.Assign(url, Expression.Call(null, methodInfo, args)),
				Expression.Assign(client, DryExpression.Call(Expression.Variable(currentType, "self"), "PKWebServiceClient", "createClientWithURL", new
				{
					url,
					options
				})),
				Expression.Assign(DryExpression.Property(client, currentType, "delegate"), self),
				clientCallExpression
			);

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), block, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateClientMethod()
		{
			var client = Expression.Variable(DryType.Define("PKWebServiceClient"), "client");
			var self = DryExpression.Variable(currentType, "self");
			var options = DryExpression.Parameter(DryType.Define("NSDictionary"), "options");
			var url = Expression.Parameter(typeof(string), "urlIn");
			var parameters = new Expression[] { url, options };
			var operationQueue = DryExpression.Call(options, "objectForKey", "OperationQueue");

			var variables = new [] { client };

			var body = DryExpression.Block
			(
				variables,
				Expression.Assign(client, DryExpression.StaticCall("PKWebServiceClient", "PKWebServiceClient", "clientWithURL", new
				{
					url = DryExpression.New("NSURL", "initWithString", url),
					options = options,
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
			var message = Expression.Parameter(typeof(string), "andMessage");
			
			var parameters = new Expression[]
			{
				client,
				errorCode,
				message
			};

			var clientOptions = DryExpression.Property(client, DryType.Define("NSDictionary"), "options");

			var response = DryExpression.Variable(DryType.Define("id"), "response");
			var responseClass = DryExpression.Call(clientOptions, "Class", "objectForKey", "$ResponseClass");
			var responseStatus = DryExpression.Call(response, "ResponseStatus", "responseStatus", null);
			var newResponseStatus = DryExpression.New("ResponseStatus", "init", null);

			var body = DryExpression.Block
			(
				new [] { response },
				Expression.Assign(response, DryExpression.Call(DryExpression.Call(responseClass, response.Type, "alloc", null), response.Type, "init", null)),
				Expression.IfThen(Expression.IsTrue(Expression.Equal(responseStatus, Expression.Constant(null, responseStatus.Type))), DryExpression.Block(DryExpression.Call(response, "setResponseStatus", newResponseStatus))),
				DryExpression.Call(responseStatus, typeof(string), "setErrorCode", errorCode),
				DryExpression.Call(responseStatus, typeof(string), "setMessage", message),
				Expression.Return(Expression.Label(), response)
			);

			return new MethodDefinitionExpression("webServiceClient", new ReadOnlyCollection<Expression>(parameters), DryType.Define("id"), body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateSerializeRequestMethod()
		{
			var error = DryExpression.Variable("NSError", "error");
			var retval = DryExpression.Variable("NSData", "retval");
			var dictionary = DryExpression.Variable("NSDictionary", "dictionary");
			var client = Expression.Parameter(DryType.Define("PKWebServiceClient"), "client");
			var requestObject = Expression.Parameter(DryType.Define("id"), "serializeRequest");
			
			var allPropertiesAsDictionary = DryExpression.Call(requestObject, "NSDictionary", "allPropertiesAsDictionary", null);

			var parameters = new[] { new DryParameterInfo(DryType.Define("NSDictionary"), "obj"), new DryParameterInfo(typeof(int), "options"), new DryParameterInfo(DryType.Define("NSError"), "error", true) };
			var methodInfo = new DryMethodInfo(DryType.Define("NSJSONSerialization"), DryType.Define("NSData"), "dataWithJSONObject", parameters, true);

			var serializedDataObject = Expression.Call(methodInfo, dictionary, Expression.Constant(0), error);

			var body = DryExpression.Block
			(
				new[] { error, retval, dictionary },
				Expression.Assign(dictionary, allPropertiesAsDictionary),
				Expression.Assign(retval, serializedDataObject),
				Expression.Return(Expression.Label(), retval)
			);

			return new MethodDefinitionExpression
			(
				"webServiceClient",
				new [] { client, requestObject }.ToReadOnlyCollection<Expression>(),
				retval.Type,
				body,
				false,
				null
			);
		}

		protected virtual MethodDefinitionExpression CreateParseResultMethod()
		{
			var self = Expression.Variable(currentType, "self");
			var client = Expression.Parameter(DryType.Define("PKWebServiceClient"), "client");
			var data = Expression.Parameter(DryType.Define("NSData"), "parseResult");
			var contentType = Expression.Parameter(typeof(string), "withContentType");
			var statusCode = Expression.Parameter(typeof(int), "andStatusCode");
			var error = DryExpression.Variable("NSError", "error");
			var propertyDictionary = DryExpression.Variable("NSDictionary", "propertyDictionary");
			var responseClass = DryExpression.Variable("Class", "responseClass");
			var value = DryExpression.Variable(DryType.Define("id"), "value");
			var response = DryExpression.Variable(DryType.Define("id"), "response");

			var parameters = new Expression[]
			{
				client,
				data,
				contentType,
				statusCode
			};

			var jsonObjectWithDataParameters = new[] { new DryParameterInfo(DryType.Define("NSDictionary"), "obj"), new DryParameterInfo(typeof(int), "options"), new DryParameterInfo(DryType.Define("NSError", true), "error", true) };
			var methodInfo = new DryMethodInfo(DryType.Define("NSJSONSerialization"), DryType.Define("NSData"), "JSONObjectWithData", jsonObjectWithDataParameters, true);

			var deserializedDictionary = Expression.Call(methodInfo, data, DryExpression.Variable(typeof(int), "NSJSONReadingAllowFragments"), error);

			var clientOptions = DryExpression.Property(client, DryType.Define("NSDictionary"), "options");

			var deserializedObject = DryExpression.Call(DryExpression.Call(responseClass, typeof(object), "alloc", null), typeof(object), "initWithPropertyDictionary", new
			{
				propertyDictionary = deserializedDictionary
			});

			var parseErrorResult = DryExpression.Call(self, "webServiceClient", new
			{
				client,
				createErrorResponseWithErrorCode = "JsonDeserializationError",
				andMessage = DryExpression.Call(error, "localizedDescription", null)
			});

			var bodyExpressions = new List<Expression>
			{
				DryExpression.Grouped
				(
					Expression.Assign(responseClass,  DryExpression.Call(clientOptions, "Class", "objectForKey", "$ResponseClass")).ToStatement()
				)
			};

			var uuidDeserialization = Expression.IfThen(Expression.Equal(responseClass, DryExpression.StaticCall("PKUUID", "Class", "class", null)), DryExpression.Block
			(
				Expression.Assign(value, DryExpression.StaticCall("PKUUID", "PKUUD", "uuidFromString", DryExpression.New(typeof(string), "initWithData", new
				{
					data,
					encoding = Expression.Variable(typeof(int), "NSUTF8StringEncoding")
				}))),
				Expression.Assign(response, DryExpression.New(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(Guid)), "init", null)),
				DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(Guid)))), "setValue" , value)
			));

			var stringDeserialization = Expression.IfThen(Expression.Equal(responseClass, DryExpression.StaticCall(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(string)), "Class", "class", null)), DryExpression.Block
			(
				Expression.Assign(value, DryExpression.New(typeof(string), "initWithData", new
				{
					data,
					encoding = Expression.Variable(typeof(int), "NSUTF8StringEncoding")
				})),
				Expression.Assign(response, DryExpression.New(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(string)), "init", null)),
				DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(string)))), "setValue", value)
			));

			var voidDeserialization = Expression.IfThen(Expression.Equal(Expression.Convert(responseClass, DryType.Define("id")), DryExpression.StaticCall(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(void)), "Class", "class", null)), Expression.Block
			(
				Expression.Return(Expression.Label(), Expression.Constant(null)).ToStatement()
			));

			var numberDeserialization = Expression.IfThen(Expression.Equal(responseClass, DryExpression.StaticCall(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(int?)), "Class", "class", null)), DryExpression.Block
			(
				new[] { value },
				Expression.Assign(value, deserializedDictionary),
				Expression.Assign(response, DryExpression.New(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(int)), "init", null)),
				Expression.IfThen
				(
					DryExpression.Call(value, typeof(bool), "isKindOfClass", DryExpression.StaticCall("NSNumber", "Class", "class", null)),
					DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(int)))), "setValue", value).ToStatement().ToBlock()
				)
			));

			var timespanDeserialization = Expression.IfThen(Expression.Equal(responseClass, DryExpression.StaticCall(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(TimeSpan?)), "Class", "class", null)), DryExpression.Block
			(
				Expression.Assign(value, DryExpression.StaticCall("PKTimeSpan", "PKTimeSpan", "fromIsoString", DryExpression.New(typeof(string), "initWithData", new
				{
					data,
					encoding = Expression.Variable(typeof(int), "NSUTF8StringEncoding")
				}))),
				Expression.Assign(response, DryExpression.New(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(TimeSpan)), "init", null)),
				DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(TimeSpan?)))), "setValue", value)
			));

			var defaultDeserialization = DryExpression.Block
			(
				Expression.Assign(propertyDictionary, deserializedDictionary),
				Expression.IfThen(Expression.IsTrue(Expression.Equal(propertyDictionary, Expression.Constant(null, propertyDictionary.Type))), Expression.Return(Expression.Label(), parseErrorResult).ToStatement().ToBlock()),
				Expression.Assign(value, deserializedObject)
			);

			Expression ifElseExpression = defaultDeserialization;

			if (currentReturnTypes.Contains(typeof(string)))
			{
				ifElseExpression = Expression.IfThenElse(stringDeserialization.Test, stringDeserialization.IfTrue, ifElseExpression);
			}

			if (currentReturnTypes.Any(c => c.GetUnwrappedNullableType().IsIntegerType() || c.GetUnwrappedNullableType() == typeof(bool)))
			{
				ifElseExpression = Expression.IfThenElse(numberDeserialization.Test, numberDeserialization.IfTrue, ifElseExpression);
			}

			if (currentReturnTypes.Contains(typeof(Guid)) || currentReturnTypes.Contains(typeof(Guid?)))
			{
				ifElseExpression = Expression.IfThenElse(uuidDeserialization.Test, uuidDeserialization.IfTrue, ifElseExpression);
			}

			if (currentReturnTypes.Contains(typeof(TimeSpan)) || currentReturnTypes.Contains(typeof(TimeSpan?)))
			{
				ifElseExpression = Expression.IfThenElse(timespanDeserialization.Test, timespanDeserialization.IfTrue, ifElseExpression);
			}

			var enumReturnTypes = currentReturnTypes.Where(c => c.IsEnum).ToList();

			foreach (var enumReturnType in enumReturnTypes)
			{
				var parsedValue = Expression.Variable(enumReturnType, "parsedValue");
				var tryParseParameters = new[] { new DryParameterInfo(typeof(string), "value"), new DryParameterInfo(enumReturnType, "outValue", true) };
				var tryParseMethodInfo = new DryMethodInfo(null, typeof(bool), TypeSystem.GetPrimitiveName(enumReturnType, true) + "TryParse", tryParseParameters, true);

				var createEnumResponse = DryExpression.Block
				(
					new[] { value },
					Expression.Assign(value, deserializedDictionary),
					Expression.Assign(response, DryExpression.New(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(typeof(int)), "init", null)),
					Expression.IfThenElse
					(
						DryExpression.Call(value, typeof(bool), "isKindOfClass", DryExpression.StaticCall("NSNumber", "Class", "class", null)),
						DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(enumReturnType))), "setValue", DryExpression.Call(value, typeof(int), "intValue", null)).ToStatement().ToBlock(),
						Expression.IfThen
						(
							DryExpression.Call(value, typeof(bool), "isKindOfClass", DryExpression.StaticCall("NSString", "Class", "class", null)),
							DryExpression.Block
							(
								new[] { parsedValue },
								Expression.IfThen
								(
									Expression.Not(Expression.Call(null, tryParseMethodInfo, Expression.Convert(value, typeof(string)), parsedValue)),
									Expression.Assign(parsedValue, Expression.Convert(Expression.Constant(0), enumReturnType)).ToStatement().ToBlock()
								),
								DryExpression.Call(Expression.Convert(response, DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(enumReturnType))), "setValue", parsedValue).ToStatement()
							)
						)
					)
				);

				ifElseExpression = Expression.IfThenElse
				(
					Expression.Equal(responseClass, DryExpression.StaticCall(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(enumReturnType), "Class", "class", null)),
					createEnumResponse,
					ifElseExpression
				);
			}

			if (currentReturnTypes.Contains(typeof(void)))
			{
				ifElseExpression = Expression.IfThenElse(voidDeserialization.Test, voidDeserialization.IfTrue, ifElseExpression);
			}
		
			var setResponseStatus = Expression.IfThen
			(
				Expression.Equal(DryExpression.Call(response, "id", "responseStatus", null), Expression.Constant(null, DryType.Define("id"))),
				DryExpression.Call(response, "setResponseStatus", DryExpression.New("ResponseStatus", "init", null)).ToStatement().ToBlock()
			);

			var populateResponseStatus = DryExpression.Call(DryExpression.Call(response, "id", "responseStatus", null), "setHttpStatus", statusCode);

			bodyExpressions.Add(ifElseExpression);
			bodyExpressions.Add(setResponseStatus);
			bodyExpressions.Add(populateResponseStatus);
			bodyExpressions.Add(DryExpression.Return(response));

			var body = DryExpression.Block(new[] { error, value, responseClass, propertyDictionary, response }, bodyExpressions.ToArray());

			return new MethodDefinitionExpression
			(
				"webServiceClient",
				new ReadOnlyCollection<Expression>(parameters),
				DryType.Define("id"),
				body,
				false,
				null
			);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentType = expression.Type;
			currentTypeDefinitionExpression = expression;
			currentReturnTypes = new HashSet<Type>(ReturnTypesCollector.CollectReturnTypes(expression));
			
			var includeExpressions = new List<IncludeExpression>
			{
				DryExpression.Include(expression.Type.Name + ".h"),
				DryExpression.Include("PKWebServiceClient.h"),
				DryExpression.Include(this.CodeGenerationContext.Options.ResponseStatusTypeName + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");
			
			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide, 
				CreateCreateClientMethod(), 
				CreateInitWithOptionsMethod(),
				this.CreateCreateErrorResponseWithErrorCodeMethod(), 
				this.CreateSerializeRequestMethod(),
				this.CreateParseResultMethod(),
				this.Visit(expression.Body)
			);

			var singleValueResponseTypes = currentReturnTypes.Where(c => c.GetUnwrappedNullableType().IsPrimitive).Select(c => DryType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(c))).ToList();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body).Append(singleValueResponseTypes).Distinct().ToList();
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			foreach (var referencedType in referencedTypes.Where(c => c is DryType && ((DryType)c).ServiceClass != null))
			{
				includeExpressions.Add(DryExpression.Include(referencedType.Name + ".h"));
			}

			var headerGroup = includeExpressions.Sorted(IncludeExpression.Compare).ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			currentType = null;

			return new TypeDefinitionExpression(expression.Type, header, body, false, null);
		}
	}
}
