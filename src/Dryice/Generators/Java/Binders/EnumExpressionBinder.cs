using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Markup;
using Dryice.Expressions;
using Platform;

namespace Dryice.Generators.Java.Binders
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

			var body = DryExpression.Block(Expression.Assign(valueMember, valParam).ToStatement());

			return new MethodDefinitionExpression(currentTypeDefinition.Type.Name, parameters.ToReadOnlyCollection(), null, body, false, null);
		}

		private Expression CreateDeserialiseStreamMethod()
		{
			var inputStream = Expression.Parameter(DryType.Define("InputStream"), "in");

			var jsonReaderType = DryType.Define("JsonReader");
			var jsonReader = Expression.Variable(jsonReaderType, "reader");
			var result = Expression.Variable(currentType, "result");

			var inputStreamReaderNew = DryExpression.New(DryType.Define("InputStreamReader"), "InputStreamReader", inputStream);

			var jsonReaderNew = DryExpression.New(jsonReaderType, "JsonReader", inputStreamReaderNew);
			var jsonReaderNextString = DryExpression.Call(jsonReader, "nextString", null);
			var resultCreate = Expression.Assign(result, DryExpression.StaticCall(currentType, currentType, "deserialize", jsonReaderNextString)).ToStatement();

			var jsonReaderClose = DryExpression.Call(jsonReader, "close", null).ToStatement();

			var exception = Expression.Variable(typeof(Exception), "exception"); ;
			var errorCodesVariable = Expression.Constant("DeserializationError", typeof(String));

			var returnResult = DryExpression.Return(result);

			var createErrorArguments = new
			{
				errorCode = errorCodesVariable,
				errorMessage = DryExpression.Call(exception, "getMessage", null),
				stackTrace = DryExpression.StaticCall(DryType.Define("Log"), typeof(String), "getStackTraceString", exception),
			};

			var resultCreateErrorResponse = Expression.Assign(result, DryExpression.StaticCall(currentType, currentType, "createErrorResponse", createErrorArguments)).ToStatement();

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

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputStream }, AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null, null, new List<Exception>() { new Exception() });
		}

		private Expression CreateDeserialiseStringMethod()
		{
			var inputString = Expression.Parameter(typeof(String), "value");

			var methodVariables = new List<ParameterExpression>();

			var methodStatements = new List<Expression>();

			if (codeGenerationContext.Options.SerializeEnumsAsStrings)
			{
				methodStatements.Add(DryExpression.Return(DryExpression.StaticCall(currentType, "valueOf", inputString)));
			}
			else
			{
				
			}

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserialize", new List<Expression>() { inputString }, AccessModifiers.Public | AccessModifiers.Static, currentType, body, false, null, null, new List<Exception>() { new Exception() });

		}

		private Expression CreateDeserialiseArrayMethod()
		{
			var jsonReader = Expression.Parameter(DryType.Define("JsonReader"), "reader");

			var result = DryExpression.Variable(new DryListType(currentType), "result");

			var resultNew = DryExpression.New(new DryListType(currentType), "DryListType", null);

			var jsonReaderNextString = DryExpression.Call(jsonReader, "nextString", null);

			var whileBody = DryExpression.Block(DryExpression.Call(result, "add", DryExpression.StaticCall(currentType, "deserialize", jsonReaderNextString)));

			var whileExpression = DryExpression.While(DryExpression.Call(jsonReader, "hasNext", null), whileBody);

			var returnResult = Expression.Return(Expression.Label(), result).ToStatement();

			var methodVariables = new List<ParameterExpression>
			{
				result
			};

			var methodStatements = new List<Expression>
			{
				Expression.Assign(result, resultNew).ToStatement(),
				DryExpression.Call(jsonReader, "beginArray", null),
				whileExpression,
				DryExpression.Call(jsonReader, "endArray", null),
				returnResult
			};

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

			return new MethodDefinitionExpression("deserializeArray", new List<Expression>() { jsonReader }, AccessModifiers.Public | AccessModifiers.Static, new DryListType(new DryType("? extends " + currentType.Name)), body, false, null, null, new List<Exception>() { new Exception() });

		}

		private Expression CreateSerialiseMethod()
		{
			var methodVariables = new List<ParameterExpression>();
			var methodStatements = new List<Expression>();

			var value = Expression.Parameter(currentTypeDefinition.Type, currentTypeDefinition.Type.Name.Uncapitalize());

			if (codeGenerationContext.Options.SerializeEnumsAsStrings)
			{
				var result = DryExpression.Variable(typeof(String), "result");

				var defaultBody = Expression.Assign(result, Expression.Constant(null, typeof(string))).ToStatement();

				var cases = new List<SwitchCase>();

				foreach (var enumValue in ((DryType)currentTypeDefinition.Type).ServiceEnum.Values)
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
				var enumValue = DryExpression.Variable(typeof(int), value.Name + "." + enumValueField.PropertyName);
				var staticToStringCall = DryExpression.StaticCall("ConvertUtils", "toString", enumValue);
				var returnResult = Expression.Return(Expression.Label(), staticToStringCall);

				methodStatements.Add(returnResult);
			}

			var body = DryExpression.Block(methodVariables.ToArray(), methodStatements.ToArray());

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
					DryExpression.Include("android.util.JsonReader"),
					DryExpression.Include("com.jaigo.androiddevkit.utils.*"),
					DryExpression.Include("java.io.InputStream"),
					DryExpression.Include("java.io.InputStreamReader"),
					DryExpression.Include("java.util.ArrayList")
				};

				var referencedTypes = ReferencedTypesCollector.CollectReferencedTypes(expression);
				referencedTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

				if (!codeGenerationContext.Options.SerializeEnumsAsStrings)
				{
					includeExpressions.Add(DryExpression.Include("com.jaigo.androiddevkit.utils.ConvertUtils"));
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
					CreateDeserialiseStreamMethod(),
					CreateDeserialiseStringMethod(),
					CreateDeserialiseArrayMethod(),
					CreateSerialiseMethod()
				};

				var body = new GroupedExpressionsExpression(bodyExpressions);

				return new TypeDefinitionExpression(expression.Type, header, body, false);
			}
			finally
			{
				currentTypeDefinition = null;
			}
		}
	}
}
