using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Objective.Binders
{
	public class GatewaySourceExpressionBinder
		: ServiceExpressionVisitor
	{
		private int methodCount = 0;
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
			var self = FickleExpression.Variable(currentType, "self");
			var super = FickleExpression.Variable(currentType, "super");
			var options = FickleExpression.Parameter("NSDictionary", "options");
			var superinit = FickleExpression.Call(super, currentType, "init", null);

			var initBlock = FickleExpression.Block(new Expression[]
			{
				Expression.Assign(FickleExpression.Property(self, "NSDictionary", "options"), options)
			});

			var body = FickleExpression.Block(new Expression[]
			{
				Expression.IfThen(Expression.NotEqual(Expression.Assign(self, superinit), Expression.Constant(null, currentType)), initBlock),
				Expression.Return(Expression.Label(), self)
			});

			return new MethodDefinitionExpression("initWithOptions", new Expression[] { options }.ToReadOnlyCollection(), FickleType.Define("id"), body, false, null);
		}

		private static readonly Regex urlParameterRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);

		public static bool IsNumericType(Type type)
		{
			return type.IsIntegerType() || type.IsRealType();
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			var methodName = method.Name.Uncapitalize();

			methodCount++;
			
			var self = Expression.Variable(currentType, "self");
			var hostname = Expression.Variable(typeof(string), "hostname");
			var options = FickleExpression.Variable("NSMutableDictionary", "localOptions");
			var url = Expression.Variable(typeof(string), "url");
			var client = Expression.Variable(FickleType.Define(this.CodeGenerationContext.Options.ServiceClientTypeName ?? "PKWebServiceClient"), "client");
			var responseType = ObjectiveBinderHelpers.GetWrappedResponseType(this.CodeGenerationContext, method.ReturnType);
			var variables = new [] { url, client, options, hostname };
			var declaredHostname = currentTypeDefinitionExpression.Attributes["Hostname"];
			var declaredPath = method.Attributes["Path"];
			var path = StringUriUtils.Combine("http://%@", declaredPath);
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

				if (type == typeof(byte) || type == typeof(short) || type == typeof(int))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%d";
				}
				else if (type == typeof(long))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%lld";
				}
				else if (type == typeof(float) || type == typeof(double))
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);

					return "%f";
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
				else if (type is FickleListType)
				{
					parameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
					args.Add(parameter);
					
					return "%@";
				}
				else if (type == typeof(Guid))
				{
					parameters.Add(Expression.Parameter(typeof(string), parameter.Name));
					var arg = FickleExpression.Call(parameter, typeof(string), "ToString", null);

					args.Add(arg);

					return "%@";
				}
				else
				{
					parameters.Add(Expression.Parameter(typeof(string), parameter.Name));
					var arg = FickleExpression.Call(parameter, typeof(string), "ToString", null);

					arg = FickleExpression.Call(arg, typeof(string), "stringByAddingPercentEscapesUsingEncoding", Expression.Variable(typeof(int), "NSUTF8StringEncoding"));

					args.Add(arg);

					return "%@";
				}
			});

			var parameterInfos = new List<FickleParameterInfo>
			{
				new ObjectiveParameterInfo(typeof(string), "s"),
				new ObjectiveParameterInfo(typeof(string), "hostname", true)
			};

			parameterInfos.AddRange(parameters.Select(c => new ObjectiveParameterInfo(c.Type, c.Name, true)));

			var methodInfo = new FickleMethodInfo(typeof(string), typeof(string), "stringWithFormat", parameterInfos.ToArray(), true);

			args.InsertRange(0, new Expression[] { Expression.Constant(objcUrl), hostname });
			
			var newParameters = new List<Expression>(method.Parameters);
			var callback = Expression.Parameter(new FickleDelegateType(typeof(void), new FickleParameterInfo(responseType, "response")), "callback");

			newParameters.Add(callback);

			Expression blockArg = Expression.Parameter(FickleType.Define("id"), "arg1");

			var returnType = method.ReturnType;

			if (ObjectiveBinderHelpers.NeedsValueResponseWrapper(method.ReturnType))
			{
				returnType = FickleType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(method.ReturnType));
			}

			var conversion = Expression.Convert(blockArg, returnType);
			var body = FickleExpression.Call(callback, "Invoke", conversion).ToStatement();
			var conversionBlock = FickleExpression.SimpleLambda(null, body, new Expression[0], blockArg);
			
			Expression clientCallExpression;

			if (httpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
			{
				clientCallExpression = FickleExpression.Call(client, "getWithCallback", conversionBlock);
			}
			else
			{
				var contentParameterName = method.Attributes["Content"];

				if (string.IsNullOrEmpty(contentParameterName))
				{
					clientCallExpression = FickleExpression.Call(client, "postWithRequestObject", new
					{
						requestObject = Expression.Constant(null, typeof(object)),
						andCallback = conversionBlock
					});
				}
				else
				{
					var content = parametersByName[contentParameterName];

					clientCallExpression = FickleExpression.Call(client, "postWithRequestObject", new
					{
						requestObject = FickleExpression.Call(self, typeof(object), this.GetNormalizeRequestMethodName(content.Type), Expression.Convert(content, typeof(object))),
						andCallback = conversionBlock
					});
				}
			}

			var error = FickleExpression.Variable("NSError", "error");

			var parseErrorResult = FickleExpression.Call(self, "webServiceClient", new
			{
				client,
				createErrorResponseWithErrorCode = "JsonDeserializationError",
				andMessage = FickleExpression.Call(error, "localizedDescription", null)
			});

			Expression parseResultBlock;

			var nsdataParam = Expression.Parameter(FickleType.Define("NSData"), "data");
			var jsonObjectWithDataParameters = new[] { new FickleParameterInfo(FickleType.Define("NSDictionary"), "obj"), new FickleParameterInfo(typeof(int), "options"), new FickleParameterInfo(FickleType.Define("NSError", true), "error", true) };
			var objectWithDataMethodInfo = new FickleMethodInfo(FickleType.Define("NSJSONSerialization"), FickleType.Define("NSData"), "JSONObjectWithData", jsonObjectWithDataParameters, true);
			var deserializedValue = Expression.Parameter(FickleType.Define("id"), "deserializedValue");

			if (method.ReturnType == typeof(void))
			{
				var responseObject = FickleExpression.Variable(responseType, "responseObject");

				parseResultBlock = FickleExpression.SimpleLambda
				(
					FickleType.Define("id"),
					FickleExpression.GroupedWide
					(
						Expression.Assign(responseObject, FickleExpression.New(responseType, "init", null)).ToStatement(),
						Expression.Assign(deserializedValue, Expression.Call(objectWithDataMethodInfo, nsdataParam, FickleExpression.Variable(typeof(int), "NSJSONReadingAllowFragments"), error)).ToStatement(),
						Expression.IfThen(Expression.Equal(deserializedValue, Expression.Constant(null)), FickleExpression.Return(parseErrorResult).ToStatement().ToBlock()),
						FickleExpression.Return(responseObject).ToStatement()
					),
					new Expression[] { deserializedValue, responseObject, error },
					nsdataParam
				);
			}
			else if (TypeSystem.IsPrimitiveType(method.ReturnType) || method.ReturnType is FickleListType)
			{
				var responseObject = FickleExpression.Variable(responseType, "responseObject");
				var needToBoxValue = ObjectiveBinderHelpers.ValueResponseValueNeedsBoxing(method.ReturnType);

				parseResultBlock = FickleExpression.SimpleLambda
				(
					FickleType.Define("id"),
					FickleExpression.GroupedWide
					(
						Expression.Assign(responseObject, FickleExpression.New(responseType, "init", null)).ToStatement(),
						Expression.Assign(deserializedValue, Expression.Call(objectWithDataMethodInfo, nsdataParam, FickleExpression.Variable(typeof(int), "NSJSONReadingAllowFragments"), error)).ToStatement(),
						Expression.IfThen(Expression.Equal(deserializedValue, Expression.Constant(null)), FickleExpression.Return(parseErrorResult).ToStatement().ToBlock()),
						PropertiesFromDictionaryExpressonBinder.GetDeserializeExpressionProcessValueDeserializer(method.ReturnType, deserializedValue, c => FickleExpression.Call(responseObject, typeof(void), "setValue", needToBoxValue  ? Expression.Convert(c, typeof(object)) : c).ToStatement()),
						FickleExpression.Return(responseObject).ToStatement()
					),
					new Expression[] { deserializedValue, responseObject, error },
					nsdataParam
				);
			}
			else
			{
				parseResultBlock = FickleExpression.SimpleLambda
				(
					FickleType.Define("id"),
					FickleExpression.GroupedWide
					(
						Expression.Assign(deserializedValue, Expression.Call(objectWithDataMethodInfo, nsdataParam, FickleExpression.Variable(typeof(int), "NSJSONReadingAllowFragments"), error)).ToStatement(),
						PropertiesFromDictionaryExpressonBinder.GetDeserializeExpressionProcessValueDeserializer(method.ReturnType, deserializedValue, c => FickleExpression.Return(c).ToStatement()),
						FickleExpression.Return(parseErrorResult).ToStatement()
					),
					new Expression[] { deserializedValue, error },
					nsdataParam
				);
			}

			parseResultBlock = FickleExpression.Call(parseResultBlock, parseResultBlock.Type, "copy", null);
			
			var block = FickleExpression.Block
			(
				variables,
				Expression.Assign(callback, FickleExpression.Call(callback, callback.Type, "copy", null)),
				Expression.Assign(options, FickleExpression.Call(FickleExpression.Property(self, FickleType.Define("NSDictionary"), "options"), "NSMutableDictionary", "mutableCopyWithZone", new
				{
					zone = Expression.Constant(null, FickleType.Define("NSZone"))
				})),
				FickleExpression.Call(options, typeof(void), "setObject", new { value = FickleExpression.StaticCall(responseType, "class", null), forKey = "$ResponseClass" }).ToStatement(),
				Expression.Assign(hostname, FickleExpression.Call(options, typeof(string), "objectForKey", Expression.Constant("Hostname"))),
				Expression.IfThen(Expression.Equal(hostname, Expression.Constant(null)), Expression.Assign(hostname, Expression.Constant(declaredHostname)).ToStatement().ToBlock()),
				FickleExpression.Grouped
				(
					FickleExpression.Call(options, "setObject", new
					{
						obj = parseResultBlock,
						forKey = "$ParseResultBlock"
					}).ToStatement(),
					method.ReturnType.GetUnwrappedNullableType() == typeof(bool) ?
					FickleExpression.Call(options, "setObject", new
					{
						obj = Expression.Convert(Expression.Constant(1), typeof(object))
					}).ToStatement() : null
				),
				Expression.Assign(url, Expression.Call(null, methodInfo, args)),
				Expression.Assign(client, FickleExpression.Call(Expression.Variable(currentType, "self"), "PKWebServiceClient", "createClientWithURL", new
				{
					url,
					options
				})),
				Expression.Assign(FickleExpression.Property(client, currentType, "delegate"), self),
				clientCallExpression
			);

			return new MethodDefinitionExpression(methodName, newParameters.ToReadOnlyCollection(), typeof(void), block, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateClientMethod()
		{
			var client = Expression.Variable(FickleType.Define("PKWebServiceClient"), "client");
			var self = FickleExpression.Variable(currentType, "self");
			var options = FickleExpression.Parameter(FickleType.Define("NSDictionary"), "options");
			var url = Expression.Parameter(typeof(string), "urlIn");
			var parameters = new Expression[] { url, options };
			var operationQueue = FickleExpression.Call(options, "objectForKey", "OperationQueue");

			var variables = new [] { client };

			var body = FickleExpression.Block
			(
				variables,
				Expression.Assign(client, FickleExpression.StaticCall("PKWebServiceClient", "PKWebServiceClient", "clientWithURL", new
				{
					url = FickleExpression.New("NSURL", "initWithString", url),
					options = options,
					operationQueue
				})),
				Expression.Return(Expression.Label(), client)
			);

			return new MethodDefinitionExpression("createClientWithURL", new ReadOnlyCollection<Expression>(parameters), FickleType.Define("PKWebServiceClient"), body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateErrorResponseWithErrorCodeMethod()
		{
			var client = Expression.Parameter(FickleType.Define("PKWebServiceClient"), "client");
			var errorCode = Expression.Parameter(typeof(string), "createErrorResponseWithErrorCode");
			var message = Expression.Parameter(typeof(string), "andMessage");
			
			var parameters = new Expression[]
			{
				client,
				errorCode,
				message
			};

			var clientOptions = FickleExpression.Property(client, FickleType.Define("NSDictionary"), "options");
			var response = FickleExpression.Variable(FickleType.Define("id"), "response");
			var responseClass = FickleExpression.Call(clientOptions, "Class", "objectForKey", "$ResponseClass");
			var responseStatus = FickleExpression.Call(response, "ResponseStatus", "responseStatus", null);
			var newResponseStatus = FickleExpression.New("ResponseStatus", "init", null);

			var body = FickleExpression.Block
			(
				new [] { response },
				Expression.Assign(response, FickleExpression.Call(FickleExpression.Call(responseClass, response.Type, "alloc", null), response.Type, "init", null)),
				Expression.IfThen(Expression.IsTrue(Expression.Equal(responseStatus, Expression.Constant(null, responseStatus.Type))), FickleExpression.Block(FickleExpression.Call(response, "setResponseStatus", newResponseStatus))),
				FickleExpression.StatementisedGroupedExpression
				(
					FickleExpression.Call(responseStatus, typeof(string), "setErrorCode", errorCode),
					FickleExpression.Call(responseStatus, typeof(string), "setMessage", message)
				),
				Expression.Return(Expression.Label(), response)
			);

			return new MethodDefinitionExpression("webServiceClient", new ReadOnlyCollection<Expression>(parameters), FickleType.Define("id"), body, false, null);
		}

		private string GetNormalizeRequestMethodName(Type forType)
		{
			forType = forType.GetFickleListElementType() ?? forType;
			forType = forType.GetUnwrappedNullableType();

			if (forType == typeof(TimeSpan) || forType == typeof(Guid) || forType == typeof(bool) || forType.IsEnum)
			{
				return "normalizeRequest" + forType.Name;
			}
			else if (forType.IsNumericType())
			{
				return "normalizeRequestNumber";
			}
			else
			{
				return "normalizeRequestObject";
			}
		}

		protected virtual MethodDefinitionExpression CreateNormalizeRequestObjectMethod(Type forType)
		{
			var requestObject = Expression.Parameter(FickleType.Define("id"), "serializeRequest");
			var newArray = FickleExpression.Variable("NSMutableArray", "newArray");
			var name = this.GetNormalizeRequestMethodName(forType);
			var value = FickleExpression.Variable("id", "value");
			var item = Expression.Variable(FickleType.Define("id"), "item");

			Expression processing;
			
			if (forType == typeof(TimeSpan))
			{
				processing = FickleExpression.Call(Expression.Convert(item, typeof(TimeSpan)), typeof(string), "ToString", null);
			}
			else if (forType == typeof(Guid))
			{
				processing = FickleExpression.Call(Expression.Convert(item, typeof(Guid)), typeof(string), "ToString", null);
			}
			else if (forType.IsEnum)
			{
				processing = Expression.Convert(FickleExpression.Call(item, typeof(int), "intValue", null), forType);

				if (this.CodeGenerationContext.Options.SerializeEnumsAsStrings)
				{
					processing = FickleExpression.StaticCall((Type)null, typeof(string), forType.Name + "ToString", processing);
				}
				else
				{
					processing = Expression.Convert(processing, typeof(object));
				}
			}
			else if (forType == typeof(bool))
			{
				processing = Expression.Condition
				(
					Expression.Equal(FickleExpression.Call(item, typeof(bool), "boolValue", null), Expression.Constant(true)),
					Expression.Constant("true"),
					Expression.Constant("false")
				);
			}
			else if (forType.IsNumericType())
			{
				processing = Expression.Convert(item, FickleType.Define("NSNumber"));
			}
			else
			{
				var allPropertiesAsDictionary = FickleExpression.Call(item, "NSDictionary", "allPropertiesAsDictionary", null);

				processing = allPropertiesAsDictionary;
			}

			var isArray = Expression.Variable(typeof(bool), "isArray");
			var array = Expression.Variable(FickleType.Define("NSArray"), "array");

			processing = Expression.IfThenElse
			(
				isArray,
				FickleExpression.Block
				(
					new [] { newArray },
					Expression.Assign(array, requestObject),
					Expression.Assign(newArray, FickleExpression.New("NSMutableArray", "initWithCapacity", FickleExpression.Call(array, typeof(int), "count", null))),
					FickleExpression.ForEach
					(
						item,
						array,
						FickleExpression.Call(newArray, typeof(void), "addObject", processing).ToStatement().ToBlock()
					),
					Expression.Assign(value, newArray)
				),
				FickleExpression.Block
				(
					new [] { item },
					Expression.Assign(item, requestObject),
					Expression.Assign(value, processing)
				)
			);

			var body = FickleExpression.Block
			(
				new[] { array, isArray, value },
				Expression.Assign(isArray, FickleExpression.Call(requestObject, typeof(bool), "isKindOfClass", FickleExpression.StaticCall("NSArray", "Class", "class", null))),
				processing,
				Expression.Return(Expression.Label(), value)
			);

			return new MethodDefinitionExpression
			(
				name,
				new[] { requestObject }.ToReadOnlyCollection<Expression>(),
				FickleType.Define("id"),
				body,
				false,
				null
			);
		}

		protected virtual MethodDefinitionExpression CreateSerializeRequestMethod()
		{
			var error = FickleExpression.Variable("NSError", "error");
			var retval = FickleExpression.Variable("NSData", "retval");
			var client = Expression.Parameter(FickleType.Define("PKWebServiceClient"), "client");
			var requestObject = Expression.Parameter(FickleType.Define("id"), "serializeRequest");
			var parameters = new[] { new FickleParameterInfo(FickleType.Define("NSDictionary"), "obj"), new FickleParameterInfo(FickleType.Define("NSJSONWritingOptions", false, true), "options"), new FickleParameterInfo(FickleType.Define("NSError"), "error", true) };
			var methodInfo = new FickleMethodInfo(FickleType.Define("NSJSONSerialization"), FickleType.Define("NSData"), "dataWithJSONObject", parameters, true);

			var body = FickleExpression.Block
			(
				new[] { error, retval },
				Expression.Assign(retval, Expression.Call(methodInfo, requestObject, Expression.Convert(Expression.Variable(typeof(int), "NSJSONReadingAllowFragments"), FickleType.Define("NSJSONWritingOptions", false, true)), error)),
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
			var client = Expression.Parameter(FickleType.Define("PKWebServiceClient"), "client");
			var data = Expression.Parameter(FickleType.Define("NSData"), "parseResult");
			var contentType = Expression.Parameter(typeof(string), "withContentType");
			var statusCode = Expression.Parameter(typeof(int), "andStatusCode");
			var response = FickleExpression.Variable(FickleType.Define("id"), "response");
			var options = FickleExpression.Property(client, "NSDictionary", "options");

			var parameters = new Expression[]
			{
				client,
				data,
				contentType,
				statusCode
			};

			var bodyExpressions = new List<Expression>();
			var delegateType = new FickleDelegateType(FickleType.Define("id"), new FickleParameterInfo(FickleType.Define("NSData"), "data"));
			var block = Expression.Variable(delegateType, "block");
			
			bodyExpressions.Add(Expression.Assign(block, FickleExpression.Call(options, FickleType.Define("id"), "objectForKey", Expression.Constant("$ParseResultBlock"))));
			bodyExpressions.Add(Expression.Assign(response,  FickleExpression.Call(block, "id", "Invoke", data)).ToStatement());
		
			var setResponseStatus = Expression.IfThen
			(
				Expression.Equal(FickleExpression.Call(response, "id", "responseStatus", null), Expression.Constant(null, FickleType.Define("id"))),
				FickleExpression.Call(response, "setResponseStatus", FickleExpression.New("ResponseStatus", "init", null)).ToStatement().ToBlock()
			);

			var populateResponseStatus = FickleExpression.Call(FickleExpression.Call(response, "id", "responseStatus", null), "setHttpStatus", statusCode);

			bodyExpressions.Add(setResponseStatus);
			bodyExpressions.Add(populateResponseStatus);
			bodyExpressions.Add(FickleExpression.Return(response));

			var body = FickleExpression.Block
			(
				new[] { response, block },
				bodyExpressions.ToArray()
			);

			return new MethodDefinitionExpression
			(
				"webServiceClient",
				new ReadOnlyCollection<Expression>(parameters),
				FickleType.Define("id"),
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
				FickleExpression.Include(expression.Type.Name + ".h"),
				FickleExpression.Include("PKWebServiceClient.h"),
				FickleExpression.Include("NSArray+PKExtensions.h"),
				FickleExpression.Include(this.CodeGenerationContext.Options.ResponseStatusTypeName + ".h")
			};

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var expressions = new List<Expression>
			{
				CreateCreateClientMethod(),
				CreateInitWithOptionsMethod()
			};

			var rawParameterTypes = ParameterTypesCollector.Collect(expression, c => c.Attributes["Method"].EqualsIgnoreCase("post") && c.Attributes.ContainsKey("Content")).Select(c => c.GetFickleListElementType() ?? c).Select(c => c.GetUnwrappedNullableType()).Distinct();
			
			foreach (var type in rawParameterTypes.Select(c => new { Type = c, Name = this.GetNormalizeRequestMethodName(c)}).GroupBy(c => c.Name).Select(c => c.First().Type))
			{
				expressions.Add(this.CreateNormalizeRequestObjectMethod(type));
			}

			expressions.Add(this.Visit(expression.Body));
			
			if (methodCount > 0)
			{
				expressions.Add(this.CreateCreateErrorResponseWithErrorCodeMethod());
				expressions.Add(this.CreateParseResultMethod());
			}

			expressions.Add(this.CreateSerializeRequestMethod());
			

			var body = GroupedExpressionsExpression.FlatConcat
			(
				GroupedExpressionsExpressionStyle.Wide,
				expressions.ToArray()
			);

			var singleValueResponseTypes = currentReturnTypes.Where(c => c.GetUnwrappedNullableType().IsPrimitive).Select(c => FickleType.Define(ObjectiveBinderHelpers.GetValueResponseWrapperTypeName(c))).ToList();

			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(body).Concat(singleValueResponseTypes).Distinct().ToList();
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			foreach (var referencedType in referencedTypes.Where(c => c is FickleType && ((FickleType)c).ServiceClass != null))
			{
				includeExpressions.Add(FickleExpression.Include(referencedType.Name + ".h"));
			}

			var headerGroup = includeExpressions.Sorted(IncludeExpression.Compare).ToGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			currentType = null;

			return new TypeDefinitionExpression(expression.Type, header, body, false, null);
		}
	}
}
