//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.IO;
using Platform;
using System.Linq;
using System.Linq.Expressions;
using Sublimate.Expressions;
using Sublimate.Model;

namespace Sublimate.Generators.Objective
{
	[PrimitiveTypeName(PrimitiveType.Byte, "uint8_t")]
	[PrimitiveTypeName(PrimitiveType.Char, "unichar")]
	[PrimitiveTypeName(PrimitiveType.Int, "int32_t")]
	[PrimitiveTypeName(PrimitiveType.Short, "int_16_t")]
	[PrimitiveTypeName(PrimitiveType.Long, "int64_t")]
	[PrimitiveTypeName(PrimitiveType.Guid, "PKUUID*")]
	[PrimitiveTypeName(PrimitiveType.DateTime, "NSDate*")]
	[PrimitiveTypeName(PrimitiveType.TimeSpan, "PKTimeSpan*")]
	[PrimitiveTypeName(PrimitiveType.String, "NSString*")]
	public class ObjectiveCodeGenerator
		: BraceLanguageStyleSourceCodeGenerator
	{
		public ObjectiveCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override void Write(ServiceType serviceType)
		{
			base.Write(serviceType);

			if (!serviceType.IsPrimitive)
			{
				base.Write("*");
			}
		}

		protected override Expression VisitIncludeStatementExpresson(IncludeStatementExpression expression)
		{
			this.Write("#include \"");
			this.Write(expression.FileName);
			this.WriteLine("\"");

			return expression;
		}

		protected override Expression VisitReferencedTypeExpresson(ReferencedTypeExpression expression)
		{
			this.Write("@class ");
			this.Write(expression.ReferencedType.Name);
			this.WriteLine(';');

			return base.VisitReferencedTypeExpresson(expression);
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			this.Visit(expression.Header);

			this.WriteLine();

			if (expression.IsPredeclaration)
			{
				this.Write("@interface ");
				this.Write(expression.Name);
				this.Write(" : ");
				this.Write(expression.BaseType.Name);

				if (expression.InterfaceTypes != null && expression.InterfaceTypes.Count > 0)
				{
					this.Write("<");
					this.Write(expression.InterfaceTypes.Select(c => c.Name).ToList().JoinToString(", "));
					this.Write(">");
				}
			}
			else
			{
				this.Write("@implementation ");
				this.Write(expression.Name);
			}

			this.WriteLine();
			this.WriteLine();

			this.Visit(expression.Body);

			this.WriteLine();
			this.WriteLine("@end");

			return expression;
		}

		protected override Expression VisitParameterDefinitionExpression(ParameterDefinitionExpression parameter)
		{
			if (parameter.Index == 0)
			{
				this.Write(parameter.ParameterName);
			}

			this.Write('(');
			this.Write(parameter.ParameterType);
			this.Write(')');
			this.Write(':');

			return parameter;
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			if (!property.IsPredeclatation)
			{
				return property;
			}

			this.Write(@"@property (readwrite) ");
			this.Write(property.PropertyType);
			this.Write(' ');
			this.Write(property.PropertyName);
			this.WriteLine(';');

			return property;
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			this.Write(method.ReturnType);
			this.WriteSpace();
			this.Write(method.Name);

			if (method.Parameters != null)
			{
				this.VisitExpressionList(method.Parameters);
			}

			if (!string.IsNullOrEmpty(method.RawAttributes) && method.IsPredeclatation)
			{
				this.Write(" __attribute__({0})", method.RawAttributes);
			}

			if (method.IsPredeclatation)
			{
				this.WriteLine(';');
			}

			return method;
		}
	}
}
