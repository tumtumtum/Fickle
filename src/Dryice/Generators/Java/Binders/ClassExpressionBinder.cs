using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Dryice.Expressions;
using Dryice.Generators.Objective;
using Dryice.Model;
using Platform;
using Platform.Text;

namespace Dryice.Generators.Java.Binders
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

			var thisProperty = DryExpression.Variable(property.PropertyType, "this." + name);

			var getterBody = DryExpression.Block
			(
				new Expression[] { DryExpression.Return(thisProperty) }
			);

			var setterParam = DryExpression.Parameter(property.PropertyType, name);

			var setterBody = DryExpression.Block
			(
				new Expression[] { Expression.Assign(thisProperty, setterParam) }
			);

			var propertyGetter = new MethodDefinitionExpression("get" + property.PropertyName, new List<Expression>(), AccessModifiers.Public, property.PropertyType, getterBody, false);
			var propertySetter = new MethodDefinitionExpression("set" + property.PropertyName, new List<Expression> { setterParam }, AccessModifiers.Public, typeof(void), setterBody, false);

			return new Expression[] { propertyGetter, propertySetter }.ToStatementisedGroupedExpression();
		}

		private Expression CreateDeserializeStreamMethod()
		{
			var inputStream = Expression.Parameter(DryType.Define("InputStream"), "in");

			var jsonReaderType = DryType.Define("JsonReader");
			var jsonReader = Expression.Variable(jsonReaderType, "reader");

			var inputStreamReaderNew = DryExpression.New(DryType.Define("InputStreamReader"), "InputStreamReader", inputStream);
			var jsonReaderNew = DryExpression.New(jsonReaderType, "JsonReader", inputStreamReaderNew); 

			var self = DryExpression.Variable(currentType, "this");
			var resultCreate = DryExpression.Call(self, currentType, "deserialize", jsonReader).ToStatement();

			var jsonReaderClose = DryExpression.Call(jsonReader, "close", null).ToStatement();

			var exception = Expression.Variable(typeof(Exception), "exception"); ;

			Expression handleError;

			if (CurrentTypeIsResponseType())
			{
				var responseStatusType = DryType.Define("ResponseStatus");
				var responseStatus = DryExpression.Variable(responseStatusType, "responseStatus");
				var newResponseStatus = Expression.Assign(responseStatus, Expression.New(responseStatusType));

				handleError = DryExpression.Grouped(new Expression[]
				{
					newResponseStatus,
					DryExpression.Call(responseStatus, "setErrorCode", Expression.Constant("DeserializationError", typeof(String))),
					DryExpression.Call(responseStatus, "setMessage", DryExpression.Call(exception, typeof(String), "getMessage", null)),
					DryExpression.Call(responseStatus, "setStackTrace", DryExpression.StaticCall("Log", "getStackTraceString", exception))
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

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputStream }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserializeReaderMethod()
		{
			var jsonReader = Expression.Parameter(DryType.Define("JsonReader"), "reader");

			var conditionNull = Expression.MakeBinary(ExpressionType.Equal, DryExpression.Call(jsonReader, typeof(Enum), "peek", null),
				DryExpression.Variable(typeof(Enum), "JsonToken.NULL"));

			var actionNull = DryExpression.Block(
				new Expression[]
				{
					DryExpression.Call(jsonReader, "skipValue", null),
					Expression.Continue(Expression.Label())
				});

			var self = DryExpression.Variable(currentType, "this");

			var whileStatements = new List<Expression>
			{
				Expression.IfThen(conditionNull, actionNull),
				DryExpression.Call(self, "deserializeElement", jsonReader)
			};

			var whileBody = DryExpression.Block(whileStatements.ToArray());

			var whileExpression = DryExpression.While(DryExpression.Call(jsonReader, "hasNext", null), whileBody);

			var methodVariables = new List<ParameterExpression>();

			var methodStatements = new List<Expression>
			{
				DryExpression.Call(jsonReader, "beginObject", null),
				whileExpression,
				DryExpression.Call(jsonReader, "endObject", null),
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { jsonReader }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserializeElementMethod()
		{
			var jsonReader = Expression.Parameter(DryType.Define("JsonReader"), "reader");

			var jsonElementName = Expression.Variable(typeof(String), "elementName");

			Expression ifThenElseExpression;

			if (currentTypeDefinition.Type.BaseType != null && currentTypeDefinition.Type.BaseType != typeof (Object))
			{
				var superType = currentTypeDefinition.Type.BaseType;
				var super = Expression.Variable(superType, "super");
				
				ifThenElseExpression = DryExpression.Block(DryExpression.Call(super, "deserializeElement", jsonReader));
			}
			else
			{
				ifThenElseExpression = DryExpression.Block(DryExpression.Call(jsonReader, "skipValue", null));
			}

			var self = Expression.Variable(currentType, "this");

			foreach (var serviceProperty in ((DryType)currentTypeDefinition.Type).ServiceClass.Properties)
			{
				Expression action = null;

				var propertyType = codeGenerationContext.ServiceModel.GetTypeFromName(serviceProperty.TypeName);

				if (propertyType is DryListType)
				{
					var listItemType = ((DryListType)propertyType).ListElementType;

					if (listItemType is DryNullable)
					{
						listItemType = listItemType.GetUnderlyingType();
					}

					if (listItemType.IsEnum)
					{
						var convertDtoCall = DryExpression.StaticCall(listItemType.Name, "deserializeArray", jsonReader);
						action = DryExpression.Block(DryExpression.Call(self, "set" + serviceProperty.Name, convertDtoCall));
					}
					else
					{
						var result = DryExpression.Variable(new DryListType(listItemType), serviceProperty.Name.Uncapitalize());

						var resultNew = DryExpression.New(new DryListType(listItemType), "DryListType", null);

						var jsonReaderNextString = DryExpression.Call(jsonReader, typeof(String), "nextString", null);

						Expression whileBody;

						if (TypeSystem.IsPrimitiveType(listItemType))
						{
							whileBody = DryExpression.Block(
								DryExpression.Call(result, "add", Expression.Convert(Expression.Convert(jsonReaderNextString, typeof(Object)), listItemType))
								);
						}
						else
						{
							var objectToDeserialize = DryExpression.Variable(listItemType, listItemType.Name.Uncapitalize());

							var objectNew = Expression.Assign(objectToDeserialize, Expression.New(listItemType));

							var whileVariables = new List<ParameterExpression>
							{
								objectToDeserialize
							};

							var whileStatements = new List<Expression>
							{
								objectNew,
								DryExpression.Call(objectToDeserialize, "deserialize", jsonReader),
								DryExpression.Call(result, "add", objectToDeserialize)
							};

							whileBody = DryExpression.Block(whileVariables.ToArray(), whileStatements.ToArray());
						}

						var whileExpression = DryExpression.While(DryExpression.Call(jsonReader, "hasNext", null), whileBody);

						var setResult = DryExpression.Call(self, "set" + serviceProperty.Name, result).ToStatement();

						var variables = new List<ParameterExpression>
						{
							result
						};

						var statements = new List<Expression>
						{
							Expression.Assign(result, resultNew).ToStatement(),
							DryExpression.Call(jsonReader, "beginArray", null),
							whileExpression,
							DryExpression.Call(jsonReader, "endArray", null),
							setResult
						};

						action = DryExpression.Block(variables.ToArray(), statements.ToArray());
					}
				}
				else if (TypeSystem.IsNotPrimitiveType(propertyType))
				{
					var value = DryExpression.Variable(propertyType, "value");

					var valueNew = Expression.Assign(value, Expression.New(propertyType));

					var convertDtoCall = DryExpression.Call(value, "deserialize", jsonReader);

					var variables = new List<ParameterExpression>
					{
						value
					};

					var statements = new List<Expression>
					{
						valueNew,
						convertDtoCall,
						DryExpression.Call(self, "set" + serviceProperty.Name, value)
					}.ToStatementisedGroupedExpression();

					action = Expression.Block(variables.ToArray(), statements);
				}
				else if (propertyType.GetUnwrappedNullableType().IsEnum)
				{
					var getPrimitiveElementCall = DryExpression.Call(jsonReader, "nextString", null);
					var convertDtoCall = DryExpression.StaticCall(serviceProperty.TypeName, "deserialize", getPrimitiveElementCall);
					action = DryExpression.Block(DryExpression.Call(self, "set" + serviceProperty.Name, convertDtoCall));
				}
				else
				{
					var getPrimitiveElementCall = DryExpression.Call(jsonReader, "nextString", null);
					var convertPrimitiveCall = DryExpression.StaticCall(propertyType, SourceCodeGenerator.ToObjectMethod, getPrimitiveElementCall);
					action = DryExpression.Block(DryExpression.Call(self, "set" + serviceProperty.Name, convertPrimitiveCall));
				}

				var condition = DryExpression.Call(jsonElementName, typeof(Boolean), "equals", Expression.Constant(serviceProperty.Name, typeof(String)));

				var currentExpression = Expression.IfThenElse(condition, action, ifThenElseExpression);

				ifThenElseExpression = currentExpression;
			}

			var methodVariables = new List<ParameterExpression>
			{
				jsonElementName
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(jsonElementName, DryExpression.Call(jsonReader, typeof(String), "nextName", null)).ToStatement(),
				ifThenElseExpression
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserializeElement", new List<Expression>() { jsonReader }, AccessModifiers.Public, typeof(void), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateSerializeMethod()
		{
			var self = Expression.Parameter(currentType, "this");

			var jsonBuilder = DryType.Define("DefaultJsonBuilder");

			var jsonBuilderInstance = DryExpression.StaticCall(jsonBuilder, "instance");

			var toJsonCall = DryExpression.Call(jsonBuilderInstance, "toJson", self);

			var defaultBody = Expression.Return(Expression.Label(), toJsonCall).ToStatement();

			var body = DryExpression.Block(defaultBody);

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

			var responseStatusType = DryType.Define("ResponseStatus");

			var result = DryExpression.Variable(currentType, "result");
			var responseStatus = DryExpression.Variable(responseStatusType, "responseStatus");

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
				DryExpression.Call(responseStatus, "setErrorCode", errorCode),
				DryExpression.Call(responseStatus, "setMessage", message),
				DryExpression.Call(responseStatus, "setStackTrace", stackTrace),
				newResult,
				DryExpression.Call(result, "setResponseStatus", responseStatus),
				Expression.Return(Expression.Label(), result)
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

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
				includeExpressions.Add(DryExpression.Include("java.util.UUID"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.Date"));
			}

			var comment = new CommentExpression("This file is AUTO GENERATED");
			var namespaceExpression = new NamespaceExpression(codeGenerationContext.Options.Namespace);

			var members = new List<Expression>() { this.Visit(expression.Body) };

			if (((DryType) currentTypeDefinition.Type).ServiceClass.Properties.Count > 0)
			{
				includeExpressions.Add(DryExpression.Include("android.util.JsonReader"));
				includeExpressions.Add(DryExpression.Include("android.util.JsonToken"));
				includeExpressions.Add(DryExpression.Include("com.jaigo.androiddevkit.*"));
				includeExpressions.Add(DryExpression.Include("com.jaigo.androiddevkit.utils.*"));
				includeExpressions.Add(DryExpression.Include("java.lang.Exception"));
				includeExpressions.Add(DryExpression.Include("java.io.InputStream"));
				includeExpressions.Add(DryExpression.Include("java.io.InputStreamReader"));
				includeExpressions.Add(DryExpression.Include("java.util.ArrayList"));

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

