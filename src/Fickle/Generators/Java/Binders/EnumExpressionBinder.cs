using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Markup;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.Java.Binders
{
	public class EnumExpressionBinder
		: ServiceExpressionVisitor
	{
		private readonly CodeGenerationContext codeGenerationContext;
		private TypeDefinitionExpression currentTypeDefinition;
		private Type currentType;

		private FieldDefinitionExpression enumValueField;

		private EnumExpressionBinder(CodeGenerationContext codeGenerationContext)
		{
			this.codeGenerationContext = codeGenerationContext;
		}

		public static Expression Bind(CodeGenerationContext codeGenerationContext, Expression expression)
		{
			var binder = new EnumExpressionBinder(codeGenerationContext);

			return binder.Visit(expression);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Expression.Parameter(node.Type, currentTypeDefinition.Type.Name.Capitalize() + node.Name.Capitalize());
		}

		protected virtual Expression CreateConstructor()
		{
			var valParam = Expression.Parameter(typeof(int), "value");

			var parameters = new Expression[]
			{
				valParam
			};

			var valueMember = Expression.Variable(enumValueField.PropertyType, "this." + enumValueField.PropertyName);

			var body = FickleExpression.Block(Expression.Assign(valueMember, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinition.Type.Name, parameters.ToReadOnlyCollection(), null, body, false, null);
		}

		private Expression CreateDeserialiseStreamMethod()
		{
			var inputStream = Expression.Parameter(FickleType.Define("InputStream"), "in");

			var jsonReaderType = FickleType.Define("JsonReader");
			var jsonReader = Expression.Variable(jsonReaderType, "reader");
			var result = Expression.Variable(currentType, "result");

			var inputStreamReaderNew = FickleExpression.New(FickleType.Define("InputStreamReader"), "InputStreamReader", inputStream);

			var jsonReaderNew = FickleExpression.New(jsonReaderType, "JsonReader", inputStreamReaderNew);
			var jsonReaderNextString = FickleExpression.Call(jsonReader, "nextString", null);
			var resultCreate = Expression.Assign(result, FickleExpression.StaticCall(currentType, currentType, "deserialize", jsonReaderNextString)).ToStatement();

			var jsonReaderClose = FickleExpression.Call(jsonReader, "close", null).ToStatement();

			var exception = Expression.Variable(typeof(Exception), "exception"); ;
			var errorCodesVariable = Expression.Constant("DeserializationError", typeof(String));

			var returnResult = FickleExpression.Return(result);

			var createErrorArguments = new
			{
				errorCode = errorCodesVariable,
				errorMessage = FickleExpression.Call(exception, "getMessage", null),
				stackTrace = FickleExpression.StaticCall(FickleType.Define("Log"), typeof(String), "getStackTraceString", exception),
			};

			var resultCreateErrorResponse = Expression.Assign(result, FickleExpression.StaticCall(currentType, currentType, "createErrorResponse", createErrorArguments)).ToStatement();

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

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputStream }, AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserializeMethod()
		{
			var inputString = Expression.Parameter(typeof(String), "value");

			var methodVariables = new List<ParameterExpression>();

			var methodStatements = new List<Expression>();

			if (codeGenerationContext.Options.SerializeEnumsAsStrings)
			{
				var returnResult = FickleExpression.Return(FickleExpression.StaticCall(currentType, "valueOf", inputString));

				methodStatements.Add(returnResult);
			}
			else
			{
				var intValue = Expression.Variable(typeof (int), "intValue");

				methodVariables.Add(intValue);

				var convertInt = Expression.Assign(intValue, FickleExpression.StaticCall("ConvertUtils", typeof(int), "toint", inputString));

				methodStatements.Add(convertInt);

				Expression ifThenElseExpression = FickleExpression.Block(FickleExpression.Return(Expression.Constant(null)));

				foreach (var enumMemberExpression in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
				{
					var enumMemberName = enumMemberExpression.Name;
					var enumMemberValue = Expression.Variable(typeof(int), enumMemberName + ".value");

					var enumMemberNameExpression = Expression.Variable(typeof(int), enumMemberName);

					var condition = Expression.Equal(intValue, enumMemberValue);
					var action = FickleExpression.Block(FickleExpression.Return(enumMemberNameExpression));

					var currentExpression = Expression.IfThenElse(condition, action, ifThenElseExpression);

					ifThenElseExpression = currentExpression;
				}

				methodStatements.Add(ifThenElseExpression);
			}

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputString }, AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null, null, new List<Exception>() { new Exception() });

		}

		private Expression CreateDeserializeArrayMethod()
		{
			var jsonReader = Expression.Parameter(FickleType.Define("JsonReader"), "reader");

			var result = FickleExpression.Variable(new FickleListType(currentType), "result");

			var resultNew = FickleExpression.New(new FickleListType(currentType), "FickleListType", null);

			var jsonReaderNextString = FickleExpression.Call(jsonReader, "nextString", null);

			var whileBody = FickleExpression.Block(FickleExpression.Call(result, "add", FickleExpression.StaticCall(currentType, "deserialize", jsonReaderNextString)));

			var whileExpression = FickleExpression.While(FickleExpression.Call(jsonReader, "hasNext", null), whileBody);

			var returnResult = Expression.Return(Expression.Label(), result).ToStatement();

			var methodVariables = new List<ParameterExpression>
			{
				result
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(result, resultNew).ToStatement(),
				FickleExpression.Call(jsonReader, "beginArray", null),
				whileExpression,
				FickleExpression.Call(jsonReader, "endArray", null),
				returnResult
			};

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserializeArray", new List<Expression>() { jsonReader }, AccessModifiers.Public | AccessModifiers.Static, new FickleListType(currentType), body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateSerializeMethod()
		{
			var methodVariables = new List<ParameterExpression>();
			var methodStatements = new List<Expression>();

			var value = Expression.Parameter(currentTypeDefinition.Type, currentTypeDefinition.Type.Name.Uncapitalize());

			if (codeGenerationContext.Options.SerializeEnumsAsStrings)
			{
				var result = FickleExpression.Variable(typeof(String), "result");

				var defaultBody = Expression.Assign(result, Expression.Constant(null, typeof(string))).ToStatement();

				var cases = new List<SwitchCase>();

				foreach (var enumValue in ((FickleType)currentTypeDefinition.Type).ServiceEnum.Values)
				{
					var assignExpression = Expression.Assign(result, Expression.Constant(enumValue.Name)).ToStatement();

					cases.Add(Expression.SwitchCase(assignExpression, Expression.Constant(enumValue.Name, currentTypeDefinition.Type)));
				}

				var switchStatement = Expression.Switch(value, defaultBody, cases.ToArray());

				methodVariables.Add(result);

				methodStatements.Add(switchStatement);
				methodStatements.Add(Expression.Return(Expression.Label(), result).ToStatement());
			}
			else
			{
				var enumValue = FickleExpression.Variable(typeof(int), value.Name + "." + enumValueField.PropertyName);
				var staticToStringCall = FickleExpression.StaticCall("ConvertUtils", "toString", enumValue);
				var returnResult = Expression.Return(Expression.Label(), staticToStringCall);

				methodStatements.Add(returnResult);
			}

			var body = FickleExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("serialize", new List<Expression>() { value }, AccessModifiers.Static | AccessModifiers.Public, typeof(string), body, false, null, null);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			try
			{
				currentTypeDefinition = expression;
				currentType = expression.Type;

				enumValueField = new FieldDefinitionExpression("value", typeof(int), AccessModifiers.Private | AccessModifiers.Constant);

				var includeExpressions = new List<Expression>()
				{
					FickleExpression.Include("android.util.JsonReader"),
					FickleExpression.Include("com.jaigo.androiddevkit.DefaultJsonBuilder"),
					FickleExpression.Include("java.util.ArrayList")
				};

				var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
				referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

				if (!codeGenerationContext.Options.SerializeEnumsAsStrings)
				{
					includeExpressions.Add(FickleExpression.Include("com.jaigo.androiddevkit.utils.ConvertUtils"));
				}

				var includeStatements = includeExpressions.ToStatementisedGroupedExpression();

				var comment = new CommentExpression("This file is AUTO GENERATED");
				var namespaceExpression = new NamespaceExpression(codeGenerationContext.Options.Namespace);
				var header = new Expression[] { comment, namespaceExpression, includeStatements }.ToStatementisedGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

				var bodyExpressions = new List<Expression>()
				{
					expression.Body,
					enumValueField,
					CreateConstructor(),
					CreateDeserializeMethod(),
					CreateDeserializeArrayMethod(),
					CreateSerializeMethod(),
					JavaBinderHelpers.CreateSerializeArrayMethod(currentType)
				};

				var body = new GroupedExpressionsExpression(bodyExpressions);

				return new TypeDefinitionExpression(expression.Type, header, body, false, expression.Attributes, expression.InterfaceTypes);
			}
			finally
			{
				currentTypeDefinition = null;
			}
		}
	}
}
