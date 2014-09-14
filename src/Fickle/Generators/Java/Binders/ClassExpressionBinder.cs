using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Fickle.Expressions;
using Fickle.Generators.Objective;
using Fickle.Model;
using Platform;
using Platform.Text;

namespace Fickle.Generators.Java.Binders
{
	public class ClassExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		private TypeDefinitionExpression currentTypeDefinition;
		private Type currentType;
		private List<FieldDefinitionExpression> fieldDefinitionsForProperties = new List<FieldDefinitionExpression>();
		private HashSet<Type> serviceModelResponseTypes;

		private ClassExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;

			GetServiceModelResponseTypes();
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new ClassExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			var name = property.PropertyName.Uncapitalize();

			fieldDefinitionsForProperties.Add(new FieldDefinitionExpression(name, property.PropertyType, AccessModifiers.Protected));

			var thisProperty = FickleExpression.Variable(property.PropertyType, "this." + name);

			var getterBody = FickleExpression.Block
			(
				new Expression[] { FickleExpression.Return(thisProperty) }
			);

			var setterParam = FickleExpression.Parameter(property.PropertyType, name);

			var setterBody = FickleExpression.Block
			(
				new Expression[] { Expression.Assign(thisProperty, setterParam) }
			);

			var propertyGetter = new MethodDefinitionExpression("get" + property.PropertyName, new List<Expression>(), AccessModifiers.Public, property.PropertyType, getterBody, false);
			var propertySetter = new MethodDefinitionExpression("set" + property.PropertyName, new List<Expression> { setterParam }, AccessModifiers.Public, typeof(void), setterBody, false);

			return new Expression[] { propertyGetter, propertySetter }.ToStatementisedGroupedExpression();
		}

		private Expression CreateDeserializeStreamMethod()
		{
			var inputStream = Expression.Parameter(FickleType.Define("InputStream"), "in");

			var jsonReaderType = FickleType.Define("JsonReader");
			var jsonReader = Expression.Variable(jsonReaderType, "reader");

			var inputStreamReaderNew = FickleExpression.New(FickleType.Define("InputStreamReader"), "InputStreamReader", inputStream);
			var jsonReaderNew = FickleExpression.New(jsonReaderType, "JsonReader", inputStreamReaderNew); 

			var self = FickleExpression.Variable(currentType, "this");
			var resultCreate = FickleExpression.Call(self, currentType, "deserialize", jsonReader).ToStatement();

			var jsonReaderClose = FickleExpression.Call(jsonReader, "close", null).ToStatement();

			var exception = Expression.Variable(typeof(Exception), "exception"); ;

			Expression handleError;

			if (CurrentTypeIsResponseType())
			{
				var responseStatusType = FickleType.Define("ResponseStatus");
				var responseStatus = FickleExpression.Variable(responseStatusType, "responseStatus");
				var newResponseStatus = Expression.Assign(responseStatus, Expression.New(responseStatusType));

				handleError = FickleExpression.Grouped(new Expression[]
				{
					newResponseStatus,
					FickleExpression.Call(responseStatus, "setErrorCode", Expression.Constant("DeserializationError", typeof(String))),
					FickleExpression.Call(responseStatus, "setMessage", FickleExpression.Call(exception, typeof(String), "getMessage", null)),
					FickleExpression.Call(responseStatus, "setStackTrace", FickleExpression.StaticCall("Log", "getStackTraceString", exception))
				}.ToStatementisedGroupedExpression());
			}
			else
			{
				handleError = Expression.Throw(exception).ToStatement();
			}

			var tryCatch = Expression.TryCatchFinally(
				resultCreate,
				jsonReaderClose,
				Expression.Catch(typeof(Exception), handleError));

			var methodVariables = new List<ParameterExpression>
			{
				jsonReader
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(jsonReader, jsonReaderNew).ToStatement(),
				tryCatch
			};

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputStream }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserializeReaderMethod()
		{
			var jsonReader = Expression.Parameter(FickleType.Define("JsonReader"), "reader");

			var conditionNull = Expression.MakeBinary(ExpressionType.Equal, FickleExpression.Call(jsonReader, typeof(Enum), "peek", null),
				FickleExpression.Variable(typeof(Enum), "JsonToken.NULL"));

			var actionNull = FickleExpression.Block(
				new Expression[]
				{
					FickleExpression.Call(jsonReader, "skipValue", null),
					Expression.Continue(Expression.Label())
				});

			var self = FickleExpression.Variable(currentType, "this");

			var whileStatements = new List<Expression>
			{
				Expression.IfThen(conditionNull, actionNull),
				FickleExpression.Call(self, "deserializeElement", jsonReader)
			};

			var whileBody = FickleExpression.Block(whileStatements.ToArray());

			var whileExpression = FickleExpression.While(FickleExpression.Call(jsonReader, "hasNext", null), whileBody);

			var methodVariables = new List<ParameterExpression>();

			var methodStatements = new List<Expression>
			{
				FickleExpression.Call(jsonReader, "beginObject", null),
				whileExpression,
				FickleExpression.Call(jsonReader, "endObject", null),
			};

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { jsonReader }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserializeElementMethod()
		{
			var jsonReader = Expression.Parameter(FickleType.Define("JsonReader"), "reader");

			var jsonElementName = Expression.Variable(typeof(String), "elementName");

			Expression ifThenElseExpression;

			if (currentTypeDefinition.Type.BaseType != null && currentTypeDefinition.Type.BaseType != typeof (Object))
			{
				var superType = currentTypeDefinition.Type.BaseType;
				var super = Expression.Variable(superType, "super");
				
				ifThenElseExpression = FickleExpression.Block(FickleExpression.Call(super, "deserializeElement", jsonReader));
			}
			else
			{
				ifThenElseExpression = FickleExpression.Block(FickleExpression.Call(jsonReader, "skipValue", null));
			}

			var self = Expression.Variable(currentType, "this");

			foreach (var serviceProperty in ((FickleType)currentTypeDefinition.Type).ServiceClass.Properties)
			{
				Expression action = null;

				var propertyType = codeGenerationContext.ServiceModel.GetTypeFromName(serviceProperty.TypeName);

				if (propertyType is FickleListType)
				{
					var listItemType = ((FickleListType)propertyType).ListElementType;

					if (listItemType is FickleNullable)
					{
						listItemType = listItemType.GetUnderlyingType();
					}

					if (listItemType.IsEnum)
					{
						var convertDtoCall = FickleExpression.StaticCall(listItemType.Name, "deserializeArray", jsonReader);
						action = FickleExpression.Block(FickleExpression.Call(self, "set" + serviceProperty.Name, convertDtoCall));
					}
					else
					{
						var result = FickleExpression.Variable(new FickleListType(listItemType), serviceProperty.Name.Uncapitalize());

						var resultNew = FickleExpression.New(new FickleListType(listItemType), "FickleListType", null);

						var jsonReaderNextString = FickleExpression.Call(jsonReader, typeof(String), "nextString", null);

						Expression whileBody;

						if (TypeSystem.IsPrimitiveType(listItemType))
						{
							whileBody = FickleExpression.Block(
								FickleExpression.Call(result, "add", Expression.Convert(Expression.Convert(jsonReaderNextString, typeof(Object)), listItemType))
								);
						}
						else
						{
							var objectToDeserialize = FickleExpression.Variable(listItemType, listItemType.Name.Uncapitalize());

							var objectNew = Expression.Assign(objectToDeserialize, Expression.New(listItemType));

							var whileVariables = new List<ParameterExpression>
							{
								objectToDeserialize
							};

							var whileStatements = new List<Expression>
							{
								objectNew,
								FickleExpression.Call(objectToDeserialize, "deserialize", jsonReader),
								FickleExpression.Call(result, "add", objectToDeserialize)
							};

							whileBody = FickleExpression.Block(whileVariables.ToArray(), whileStatements.ToArray());
						}

						var whileExpression = FickleExpression.While(FickleExpression.Call(jsonReader, "hasNext", null), whileBody);

						var setResult = FickleExpression.Call(self, "set" + serviceProperty.Name, result).ToStatement();

						var variables = new List<ParameterExpression>
						{
							result
						};

						var statements = new List<Expression>
						{
							Expression.Assign(result, resultNew).ToStatement(),
							FickleExpression.Call(jsonReader, "beginArray", null),
							whileExpression,
							FickleExpression.Call(jsonReader, "endArray", null),
							setResult
						};

						action = FickleExpression.Block(variables.ToArray(), statements.ToArray());
					}
				}
				else if (TypeSystem.IsNotPrimitiveType(propertyType))
				{
					var value = FickleExpression.Variable(propertyType, "value");

					var valueNew = Expression.Assign(value, Expression.New(propertyType));

					var convertDtoCall = FickleExpression.Call(value, "deserialize", jsonReader);

					var variables = new List<ParameterExpression>
					{
						value
					};

					var statements = new List<Expression>
					{
						valueNew,
						convertDtoCall,
						FickleExpression.Call(self, "set" + serviceProperty.Name, value)
					}.ToStatementisedGroupedExpression();

					action = Expression.Block(variables.ToArray(), statements);
				}
				else if (propertyType.GetUnwrappedNullableType().IsEnum)
				{
					var getPrimitiveElementCall = FickleExpression.Call(jsonReader, "nextString", null);
					var convertDtoCall = FickleExpression.StaticCall(serviceProperty.TypeName, "deserialize", getPrimitiveElementCall);
					action = FickleExpression.Block(FickleExpression.Call(self, "set" + serviceProperty.Name, convertDtoCall));
				}
				else
				{
					var getPrimitiveElementCall = FickleExpression.Call(jsonReader, "nextString", null);
					var convertPrimitiveCall = FickleExpression.StaticCall(propertyType, SourceCodeGenerator.ToObjectMethod, getPrimitiveElementCall);
					action = FickleExpression.Block(FickleExpression.Call(self, "set" + serviceProperty.Name, convertPrimitiveCall));
				}

				var condition = FickleExpression.Call(jsonElementName, typeof(Boolean), "equals", Expression.Constant(serviceProperty.Name, typeof(String)));

				var currentExpression = Expression.IfThenElse(condition, action, ifThenElseExpression);

				ifThenElseExpression = currentExpression;
			}

			var methodVariables = new List<ParameterExpression>
			{
				jsonElementName
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(jsonElementName, FickleExpression.Call(jsonReader, typeof(String), "nextName", null)).ToStatement(),
				ifThenElseExpression
			};

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserializeElement", new List<Expression>() { jsonReader }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateSerializeMethod()
		{
			var self = Expression.Parameter(currentType, "this");

			var jsonBuilder = FickleType.Define("DefaultJsonBuilder");

			var jsonBuilderInstance = FickleExpression.StaticCall(jsonBuilder, "instance");

			var toJsonCall = FickleExpression.Call(jsonBuilderInstance, "toJson", self);

			var defaultBody = Expression.Return(Expression.Label(), toJsonCall).ToStatement();

			var body = FickleExpression.Block(defaultBody);

			return new MethodDefinitionExpression("serialize", new List<Expression>() , AccessModifiers.Public, typeof(string), body, false, null);
		}

		protected virtual MethodDefinitionExpression CreateCreateErrorResponseMethod()
		{
			var errorCode = Expression.Parameter(typeof(string), "errorCode");
			var message = Expression.Parameter(typeof(string), "errorMessage");
			var stackTrace = Expression.Parameter(typeof(string), "stackTrace");

			var parameters = new Expression[]
			{
				errorCode,
				message,
				stackTrace
			};

			var responseStatusType = FickleType.Define("ResponseStatus");

			var result = FickleExpression.Variable(currentType, "result");
			var responseStatus = FickleExpression.Variable(responseStatusType, "responseStatus");

			var newResult = Expression.Assign(result, Expression.New(currentType));
			var newResponseStatus = Expression.Assign(responseStatus, Expression.New(responseStatusType));

			var methodVariables = new List<ParameterExpression>
			{
				result,
				responseStatus
			};

			var methodStatements = new List<Expression>
			{
				newResponseStatus,
				FickleExpression.Call(responseStatus, "setErrorCode", errorCode),
				FickleExpression.Call(responseStatus, "setMessage", message),
				FickleExpression.Call(responseStatus, "setStackTrace", stackTrace),
				newResult,
				FickleExpression.Call(result, "setResponseStatus", responseStatus),
				Expression.Return(Expression.Label(), result)
			};

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("createErrorResponse", new ReadOnlyCollection<Expression>(parameters), AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentTypeDefinition = expression;
			currentType = expression.Type;
			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var includeExpressions = new List<IncludeExpression>();

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(FickleExpression.Include("java.util.UUID"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(FickleExpression.Include("java.util.Date"));
			}

			var comment = new CommentExpression("This file is AUTO GENERATED");
			var namespaceExpression = new NamespaceExpression(codeGenerationContext.Options.Namespace);

			var members = new List<Expression>() { this.Visit(expression.Body) };

			if (((FickleType) currentTypeDefinition.Type).ServiceClass.Properties.Count > 0)
			{
				includeExpressions.Add(FickleExpression.Include("android.util.JsonReader"));
				includeExpressions.Add(FickleExpression.Include("android.util.JsonToken"));
				includeExpressions.Add(FickleExpression.Include("com.jaigo.androiddevkit.*"));
				includeExpressions.Add(FickleExpression.Include("com.jaigo.androiddevkit.utils.*"));
				includeExpressions.Add(FickleExpression.Include("java.lang.Exception"));
				includeExpressions.Add(FickleExpression.Include("java.io.InputStream"));
				includeExpressions.Add(FickleExpression.Include("java.io.InputStreamReader"));
				includeExpressions.Add(FickleExpression.Include("java.util.ArrayList"));

				members.Add(CreateDeserializeStreamMethod());
				members.Add(CreateDeserializeReaderMethod());
				members.Add(CreateDeserializeElementMethod());
				members.Add(CreateSerializeMethod());
			}

			if (CurrentTypeIsResponseType())
			{
				members.Add(CreateCreateErrorResponseMethod());
			}

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, namespaceExpression, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var body = fieldDefinitionsForProperties.Concat(members).ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false);
		}

		private void GetServiceModelResponseTypes()
		{
			serviceModelResponseTypes = codeGenerationContext.ServiceModel.Gateways.SelectMany(c => c.Methods).Select(c => codeGenerationContext.ServiceModel.GetTypeFromName(c.Returns)).ToHashSet();
		}

		private bool CurrentTypeIsResponseType()
		{
			return serviceModelResponseTypes.Contains(currentType) || currentType.Name.EndsWith("ValueResponse");
		}
	}
}

