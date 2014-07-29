using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dryice.Expressions;
using Dryice.Generators.Objective;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java.Binders
{
	public class ClassExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		private Type currentType;
		private List<FieldDefinitionExpression> fieldDefinitionsForProperties = new List<FieldDefinitionExpression>();

		private ClassExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
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

			var propertyGetter = new MethodDefinitionExpression("get" + property.PropertyName, new List<Expression>(), property.PropertyType, getterBody, false);
			var propertySetter = new MethodDefinitionExpression("set" + property.PropertyName, new List<Expression> { setterParam }, typeof(void), setterBody, false);

			return new Expression[] { propertyGetter, propertySetter }.ToStatementisedGroupedExpression();
		}

		private Expression CreateDeserialiseStreamMethod()
		{
			var inputStream = Expression.Parameter(DryType.Define("InputStream"), "in");

			var jsonReaderType = DryType.Define("JsonReader");
			var jsonReader = Expression.Variable(jsonReaderType, "reader");
			var result = Expression.Variable(currentType, "result");

			var inputStreamReaderNew = DryExpression.New(DryType.Define("InputStreamReader"), "InputStreamReader", inputStream);

			var jsonReaderNew = DryExpression.New(jsonReaderType, "JsonReader", inputStreamReaderNew); 
			var resultCreate = Expression.Assign(result, DryExpression.StaticCall(currentType, currentType, "deserialise", jsonReader)).ToStatement();
			var jsonReaderClose = DryExpression.Call(jsonReader, "close", null).ToStatement();

			var exception = Expression.Variable(typeof(Exception), "exception"); ;
			var errorCodesVariable = Expression.Variable(typeof(Enum), "ServiceErrorCodes.DeserializationError");

			var returnResult = DryExpression.Return(result);

			var createErrorArguments = new
			{
				errorCode = DryExpression.Call(errorCodesVariable, typeof(String), "toString", null),
				errorMessage = DryExpression.Call(exception, "getMessage", null),
				stackTrace = DryExpression.StaticCall(DryType.Define("Log"), typeof(String), "getStackTraceString", exception),
			};

			var resultCreateErrorResponse = Expression.Assign(result, DryExpression.StaticCall(currentType, currentType, "createError", createErrorArguments)).ToStatement();

			var tryCatch = Expression.TryCatchFinally(
				resultCreate,
				jsonReaderClose,
				Expression.Catch(typeof(Exception), resultCreateErrorResponse));

			var methodVariables = new List<ParameterExpression>
			{
				result,
				jsonReader
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(jsonReader, jsonReaderNew).ToStatement(),
				tryCatch,
				returnResult
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputStream }, AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null, null);
		}

		private Expression CreateDeserialiseReaderMethod()
		{
			var value = Expression.Parameter(currentType, "value");

			var defaultBody = Expression.Return(Expression.Label(), Expression.Constant(null, typeof(string))).ToStatement();

			var body = DryExpression.Block(defaultBody);

			return new MethodDefinitionExpression("deserialize", new List<Expression>(), AccessModifiers.Public | AccessModifiers.Static, typeof(string), body, false, null, null);

		}

		private Expression CreateDeserialiseArrayMethod()
		{
			var value = Expression.Parameter(currentType, "value");

			var defaultBody = Expression.Return(Expression.Label(), Expression.Constant(null, typeof(string))).ToStatement();

			var body = DryExpression.Block(defaultBody);

			return new MethodDefinitionExpression("deserializeArray", new List<Expression>(), AccessModifiers.Public | AccessModifiers.Static, typeof(string), body, false, null, null);

		}

		private Expression CreateSerialiseMethod()
		{
			var value = Expression.Parameter(currentType, "value");

			var jsonBuilder = DryType.Define("DefaultJsonBuilder");

			var jsonBuilderInstance = DryExpression.StaticCall(jsonBuilder, "instance");

			var toJsonCall = DryExpression.Call(jsonBuilderInstance, "toJson", value);

			var defaultBody = Expression.Return(Expression.Label(), toJsonCall).ToStatement();

			var body = DryExpression.Block(defaultBody);

			return new MethodDefinitionExpression("serialize", new List<Expression>() {value}, AccessModifiers.Public | AccessModifiers.Static, typeof(string), body, false, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			currentType = expression.Type;
			var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
			referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

			var includeExpressions = referencedTypes
				.Where(ObjectiveBinderHelpers.TypeIsServiceClass)
				.Where(c => c != expression.Type.BaseType)
				.Select(c => (Expression)new ReferencedTypeExpression(c)).ToList();

			foreach (var referencedType in referencedTypes.Where(JavaBinderHelpers.TypeIsServiceClass))
			{
				includeExpressions.Add(DryExpression.Include(referencedType.Name));
			}

			var lookup = new HashSet<Type>(referencedTypes.Where(TypeSystem.IsPrimitiveType));

			if (lookup.Contains(typeof(Guid)) || lookup.Contains(typeof(Guid?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.UUID"));
			}

			if (lookup.Contains(typeof(TimeSpan)) || lookup.Contains(typeof(TimeSpan?)))
			{
				includeExpressions.Add(DryExpression.Include("java.util.Date"));
			}

			if (ObjectiveBinderHelpers.TypeIsServiceClass(expression.Type.BaseType))
			{
				includeExpressions.Add(DryExpression.Include(expression.Type.BaseType.Name));
			}

			includeExpressions.Add(DryExpression.Include("java.util.Dictionary"));

			var comment = new CommentExpression("This file is AUTO GENERATED");

			var headerGroup = includeExpressions.ToStatementisedGroupedExpression();
			var header = new Expression[] { comment, headerGroup }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			var members = new List<Expression>
			{
				this.Visit(expression.Body),
				CreateDeserialiseStreamMethod(),
				CreateDeserialiseReaderMethod(),
				CreateDeserialiseArrayMethod(),
				CreateSerialiseMethod()
			};

			var body = fieldDefinitionsForProperties.Concat(members).ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			return new TypeDefinitionExpression(expression.Type, header, body, false);
		}
	}
}
