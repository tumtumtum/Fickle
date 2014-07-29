using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Dryice.Expressions;
using Dryice.Model;
using Platform;

namespace Dryice.Generators.Java
{
	public class JavaServiceModelResponseAmender
		: ServiceModelResponseAmender
	{
		public JavaServiceModelResponseAmender(ServiceModel serviceModel, CodeGenerationOptions options)
			: base(serviceModel, options)
		{
		}

		protected override ServiceClass CreateValueResponseServiceClass(Type type)
		{
			var isNullable = DryNullable.GetUnderlyingType(type) != null;

			if (isNullable)
			{
				type = DryNullable.GetUnderlyingType(type);
			}

			var typeName = TypeSystem.GetPrimitiveName(type, true);

			var properties = new List<ServiceProperty>
			{
				new ServiceProperty()
				{
					Name = options.ResponseStatusPropertyName,
					TypeName = options.ResponseStatusTypeName
				}
			};

			if (type != typeof(void))
			{
				properties.Add
				(
					new ServiceProperty()
					{
						Name = "Value",
						TypeName = typeName
					}
				);
			}

			return new ServiceClass(JavaBinderHelpers.GetValueResponseWrapperTypeName(type), null, properties);
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

			var response = DryExpression.Variable(DryType.Define("id"), "response");
			var responseStatus = DryExpression.Call(response, "ResponseStatus", "responseStatus", null);
			var newResponseStatus = DryExpression.New("ResponseStatus", "init", null);

			var body = DryExpression.Block
			(
				new[] { response },
				Expression.IfThen(Expression.IsTrue(Expression.Equal(responseStatus, Expression.Constant(null, responseStatus.Type))), DryExpression.Block(DryExpression.Call(response, "setResponseStatus", newResponseStatus))),
				DryExpression.Call(responseStatus, typeof(string), "setErrorCode", errorCode),
				DryExpression.Call(responseStatus, typeof(string), "setMessage", message),
				Expression.Return(Expression.Label(), response)
			);

			return new MethodDefinitionExpression("createErrorResponse", new ReadOnlyCollection<Expression>(parameters), AccessModifiers.Public | AccessModifiers.Static, typeof(String), body, false, null);
		}
	}
}
