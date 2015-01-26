//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Platform;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.Objective
{
	[PrimitiveTypeName(typeof(bool), "BOOL", false)]
	[PrimitiveTypeName(typeof(bool?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(byte), "int8_t", false)]
	[PrimitiveTypeName(typeof(byte?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(char), "unichar", false)]
	[PrimitiveTypeName(typeof(char?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(short), "int16_t", false)]
	[PrimitiveTypeName(typeof(short?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(int), "int", false)]
	[PrimitiveTypeName(typeof(int?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(long), "int64_t", false)]
	[PrimitiveTypeName(typeof(long?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(float), "Float32", false)]
	[PrimitiveTypeName(typeof(float?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(double), "Float64", false)]
	[PrimitiveTypeName(typeof(double?), "NSNumber", true)]
	[PrimitiveTypeName(typeof(Guid), "PKUUID", true)]
	[PrimitiveTypeName(typeof(Guid?), "PKUUID", true)]
	[PrimitiveTypeName(typeof(DateTime), "NSDate", true)]
	[PrimitiveTypeName(typeof(DateTime?), "NSDate", true)]
	[PrimitiveTypeName(typeof(TimeSpan), "PKTimeSpan", true)]
	[PrimitiveTypeName(typeof(TimeSpan?), "PKTimeSpan", true)]
	[PrimitiveTypeName(typeof(string), "NSString", true)]
	public class ObjectiveCodeGenerator
		: BraceLanguageStyleSourceCodeGenerator
	{
		private readonly string[] reservedKeywords = new string[]
		{
			"id",
			"class",
			"void",
			"public",
			"property"
		};

		public ObjectiveCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override void Write(Type type, bool nameOnly)
		{
			var underlyingType = FickleNullable.GetUnderlyingType(type);

			if (underlyingType != null && underlyingType.BaseType == typeof(Enum))
			{
				if (nameOnly)
				{
					this.Write("NSNumber");
				}
				else
				{
					this.Write("NSNumber*");
				}

				return;
			}
			else if (type == typeof(object))
			{
				if (nameOnly)
				{
					this.Write("NSObject");
				}
				else
				{
					this.Write("NSObject*");
				}

				return;
			}
			else if (type == typeof(void))
			{
				this.Write("void");

				return;
			}
			else if (underlyingType != null && underlyingType.IsNumericType())
			{
				if (nameOnly)
				{
					this.Write("NSNumber");
				}
				else
				{
					this.Write("NSNumber*");
				}

				return;
			}
			else if (type.IsClass && type.Name == "id")
			{
				this.Write("id");

				return;
			}
			else if (type.IsClass && type.Name == "Class")
			{
				this.Write("Class");

				return;
			}
			else if (type.IsInterface)
			{
				this.Write("id<{0}>", type.Name);

				return;
			}
			else if (type.GetFickleListElementType() != null)
			{
				if (nameOnly)
				{
					this.Write("NSMutableArray");
				}
				else
				{
					this.Write("NSMutableArray*");
				}

				return;
			}
			else if (type is FickleDelegateType)
			{
				var delegateType = (FickleDelegateType)type;

				this.Write(delegateType.ReturnType, false);
				this.Write("(^)");
				this.Write("(");
				for (var i = 0; i < delegateType.Parameters.Length; i++)
				{
					this.Write(delegateType.Parameters[i].ParameterType, false);
					this.Write(' ');
					this.Write(delegateType.Parameters[i].Name);

					if (i != delegateType.Parameters.Length - 1)
					{
						this.Write(", ");
					}
				}
				this.Write(")");

				return;
			} 

			var fickleType = type as FickleType;

			if (fickleType != null)
			{
				base.Write(type, nameOnly);

				if (type.Name == "Sex")
				{
					Console.WriteLine();
				}

				if (!nameOnly)
				{
					if (fickleType.IsEnum && fickleType.IsByRef)
					{
						this.Write("*");
					}
					else if (!(fickleType.IsPrimitive || fickleType.IsEnum))
					{
						this.Write("*");
					}
				}
			}
			else
			{
				base.Write(type, nameOnly);

				if (!nameOnly && this.IsReferenceType(type))
				{
					base.Write("*");
				}
			}
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Not)
			{
				this.Write('!');
				this.Write('(');
				this.Visit(node.Operand);
				this.Write(')');
			}
			else if (node.NodeType == ExpressionType.Convert)
			{
				if ((node.Type == typeof(Guid) || node.Type == typeof(Guid?))
					&& node.Operand.Type != FickleType.Define("id"))
				{
					if (node.Operand.Type.GetUnwrappedNullableType() == typeof(Guid))
					{
						return node;
					}

					this.Write("[");
					this.Write(typeof(Guid), true);
					this.Write(" uuidFromString:");
					if (node.Operand.Type != typeof(string))
					{
						this.Write("(NSString*)");
					}
					this.Visit(node.Operand);
					this.Write("]");
				}
				else if ((node.Type.GetUnwrappedNullableType().IsNumericType()
					|| node.Type.GetUnwrappedNullableType() == typeof(bool))
					&& (node.Operand.Type == typeof(object)
					|| FickleNullable.GetUnderlyingType(node.Operand.Type) != null 
					|| node.Operand.Type.Name == "NSNumber"
					|| node.Operand.Type.Name == "id"))
				{
					var type = node.Type;

					if (type == typeof(bool))
					{
						this.Write("(BOOL)((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").boolValue");
					}
					else if (type == typeof(bool?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					} 
					else if (type == typeof(byte))
					{
						this.Write("(uint8_t)((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").intValue");
					}
					else if (type == typeof(byte?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else if (type == typeof(short))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").shortValue");
					}
					else if (type == typeof(short?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else if (type == typeof(int))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").intValue");
					}
					else if (type == typeof(int?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else if (type == typeof(long))
					{
						this.Write("(int64_t)((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").longLongValue");
					}
					else if (type == typeof(long?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else if (type == typeof(float))
					{
						this.Write("(float)((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").floatValue");
					}
					else if (type == typeof(float?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else if (type == typeof(double))
					{
						this.Write("(double)((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(").doubleValue");
					}
					else if (type == typeof(double?))
					{
						this.Write("((NSNumber*)");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else
					{
						throw new Exception("Unexpected type: " + node.Type.Name);
					}
				}
				else if (node.Type == typeof(DateTime?) || node.Type == typeof(DateTime))
				{
					this.Write("((NSString*)currentValueFromDictionary).length >= 16 ? [NSDate dateWithTimeIntervalSince1970:[[(NSString*)");
					this.Visit(node.Operand);
					this.Write(" substringWithRange:NSMakeRange(6, 10)] intValue]] : nil");
				}
				else if ((node.Type == typeof(TimeSpan?) || node.Type == typeof(TimeSpan))
					&& node.Operand.Type != FickleType.Define("id"))
				{
					this.Write("[");
					this.Write(typeof(TimeSpan), true);
					this.Write(" fromIsoString:");

					if (node.Operand.Type != typeof(string))
					{
						this.Write("(NSString*)");
					}

					this.Visit(node.Operand);
					this.Write("]");
				}
				else if ((node.Type.IsNullable() && node.Type.GetUnwrappedNullableType().IsEnum)
					&& node.Operand.Type.IsEnum)
				{
					this.Write("@(");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(object))
				{
					if (node.Operand.Type.IsNumericType(false)
					    || node.Operand.Type == typeof(bool)
						|| node.Operand.Type.IsEnum)
					{
						this.Write("@(");
						this.Visit(node.Operand);
						this.Write(")");
					}
					else
					{
						this.Visit(node.Operand);
					}
				}
				else
				{
					this.Write("((");
					this.Write(node.Type);
					this.Write(')');
					this.Write('(');
					this.Visit(node.Operand);
					this.Write(')');
					this.Write(')');
				}
			}
			else if (node.NodeType == ExpressionType.Quote)
			{
				this.Visit(node.Operand);
				this.WriteLine(';');
			}
			else if (node.NodeType == ExpressionType.IsTrue)
			{
				this.Visit(node.Operand);
			}
			else if (node.NodeType == ExpressionType.IsFalse)
			{
				this.Write('!');
				this.Visit(node.Operand);
			}

			return node;
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (!node.Type.IsValueType && node.Value == null)
			{
				this.Write("nil");

				return node;
			}

			if (node.Type == typeof(string))
			{	
				this.Write("@\"" + Convert.ToString(node.Value) + "\"");
			}
			else if (node.Type == typeof(Guid))
			{
				this.Write("[PKUUID uuidFromString:\"" + Convert.ToString(node.Value) + "\"]");
			}
			else if (node.Type == typeof(bool))
			{
				this.Write((bool)node.Value ? "YES" : "NO");
			}
			else
			{
				this.Write(Convert.ToString(node.Value));
			}

			return node;
		}

		protected override Expression VisitForEachExpression(ForEachExpression expression)
		{
			this.Write("for (");
			this.Write(expression.VariableExpression.Type);
			this.Write(" ");
			this.Visit(expression.VariableExpression);
			this.Write(" in ");
			this.Visit(expression.Target);
			this.WriteLine(")");
			this.Visit(expression.Body);
			this.WriteLine();

			return expression;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			this.Write(node.Name);

			return node;
		}

		protected virtual void WriteVariableDeclaration(ParameterExpression node)
		{
			if (node.Type is FickleDelegateType)
			{
				var delegateType = (FickleDelegateType)node.Type;

				if (delegateType.ReturnType != null)
				{
					this.Write(delegateType.ReturnType);
				}
				this.Write("(^");
				this.Write(node.Name);
				this.Write(")");
				this.Write("(");
				for (var i = 0; i < delegateType.Parameters.Length; i++)
				{
					this.Write(delegateType.Parameters[i].ParameterType, false);
					this.Write(' ');
					this.Write(delegateType.Parameters[i].Name);

					if (i != delegateType.Parameters.Length - 1)
					{
						this.Write(", ");
					}
				}
				this.Write(")");
				this.WriteLine(';');
			}
			else
			{
				this.Write(node.Type);
				this.Write(' ');
				this.Write(node.Name);
				this.WriteLine(';');
			}
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
			{
				if (node.Variables != null)
				{
					foreach (var expression in node.Variables.OrderBy(c => c.Type.Name.Length + c.Name.Length))
					{
						this.WriteVariableDeclaration(expression);
					}
				}

				if (node.Variables != null && node.Variables.Count > 0)
				{
					this.WriteLine();
				}

				this.VisitExpressionList(node.Expressions);
			}

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.NodeType == ExpressionType.MemberAccess)
			{
				this.Visit(node.Expression);
				this.Write('.');

				if (node.Member is PropertyInfo)
				{
					this.Write(node.Member.Name.Uncapitalize());
				}
				else
				{
					this.Write(node.Member.Name);
				}
			}

			return node;
		}

		protected override Expression VisitTypeBinary(TypeBinaryExpression node)
		{
			this.Write('[');
			this.Visit(node.Expression);
			this.Write(" isKindOfClass:");
			this.Write(node.TypeOperand, true);
			this.Write(".class");
			this.Write(']');

			return node;
		}

		protected override Expression VisitNew(NewExpression node)
		{
			var constructorName = node.Constructor.Name;
			
			if (constructorName == "ctor")
			{
				constructorName = "init";
			}

			this.Write('[');
			this.Write('[');
			this.Write(node.Type, true);
			this.Write(" alloc]");
			this.Write(' ');
			this.Write(constructorName);

			if (node.Arguments.Count > 0)
			{
				this.Write(':');
				this.Visit(node.Arguments[0]);

				for (var i = 1; i < node.Arguments.Count; i++)
				{
					this.Write(' ');
					this.Write(node.Constructor.GetParameters()[i].Name);
					this.Write(':');
					this.Visit(node.Arguments[i]);
				}
			}

			this.Write(']');

			return node;
		}

		public override void Generate(Expression expression)
		{
			var normalized = ReservedKeywordNormalizer.Normalize(expression, "$", reservedKeywords);

			base.Generate(DateFormatterInserter.Insert(normalized));
		}

		public override void ConvertToStringMethodCall(Expression expression)
		{
			if (expression.Type == typeof(string))
			{
				this.Visit(expression);
			}
			else if (expression.Type.IsEnum)
			{
				this.Write(expression.Type);
				this.Write("ToString(");
				this.Visit(expression);
				this.Write(")");
			}
			else if (expression.Type.GetUnwrappedNullableType().IsEnum)
			{
				this.Write(expression.Type.GetUnwrappedNullableType());
				this.Write("ToString([");
				this.Visit(expression);
				this.Write(" intValue])");
			}
			else if (expression.Type == typeof(Guid) || expression.Type == typeof(Guid?))
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" stringValueWithFormat:PKUUIDFormatCompact");
				this.Write("]");
			}
			else if (expression.Type == typeof(TimeSpan) || expression.Type == typeof(TimeSpan?))
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" toIsoString");
				this.Write("]");
			}
			else if (expression.Type == typeof(DateTime) || expression.Type == typeof(DateTime?))
			{
				this.Write("[dateFormatter stringFromDate:");
				this.Visit(expression);
				this.Write("]");
			}
			else if (expression.Type == typeof(bool))
			{
				this.Write("(");
				this.Visit(expression);
				this.Write(") ? @\"true\" : @\"false\"");
			}
			else if (expression.Type == typeof(bool?))
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" boolValue] ? @\"true\" : @\"false\"");
			}
			else if (expression.Type.GetUnwrappedNullableType().IsNumericType())
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" ");
				this.Write(" stringValue");
				this.Write("]");
			}
			else
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" description]");
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.Name == SourceCodeGenerator.ToStringMethod)
			{
				ConvertToStringMethodCall(node.Object);

				return node;
			}

			if (node.Method.Name == "Invoke" && node.Object.Type is FickleDelegateType)
			{
				this.Visit(node.Object);
				this.Write('(');

				for (var i = 0; i < node.Arguments.Count; i++)
				{
					var arg = node.Arguments[i];

					this.Visit(arg);

					if (i != node.Arguments.Count - 1)
					{
						this.Write(", ");
					}
				}

				this.Write(')');

				return node;
			}
			else if (node.Method.DeclaringType == null)
			{
				this.Write(node.Method.Name);
				this.Write('(');

				for (var i = 0; i < node.Arguments.Count; i++)
				{
					var arg = node.Arguments[i];

					if (node.Method.GetParameters()[i].IsIn)
					{
						this.Write("&");
					}

					this.Visit(arg);

					if (i != node.Arguments.Count - 1)
					{
						this.Write(", ");
					}
				}

				this.Write(')');

				return node;
			}

			this.Write('[');

			if (node.Object == null)
			{
				this.Write(node.Method.DeclaringType, true);
			}
			else
			{
				this.Visit(node.Object);
			}

			this.Write(' ');
			this.Write(node.Method.Name);
			
			if (node.Arguments.Count > 0)
			{
				this.Write(':');
				this.Visit(node.Arguments[0]);

				for (var i = 1; i < node.Arguments.Count; i++)
				{
					var parameter = node.Method.GetParameters()[i];

					if (parameter is ObjectiveParameterInfo && ((ObjectiveParameterInfo)parameter).IsCStyleParameter)
					{
						this.Write(", ");
						this.Visit(node.Arguments[i]);
					}
					else
					{
						this.Write(' ');
						this.Write(node.Method.GetParameters()[i].Name);
						this.Write(':');
						if (node.Method.GetParameters()[i].IsIn)
						{
							this.Write("&");
						}
						this.Visit(node.Arguments[i]);
					}
				}
			}

			this.Write(']');

			return node;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			switch (node.NodeType)
			{
				case ExpressionType.Or:
					this.Write("((");
					this.Visit(node.Left);
					this.Write(") || (");
					this.Visit(node.Right);
					this.Write(")");
					break;
				case ExpressionType.And:
					this.Write("((");
					this.Visit(node.Left);
					this.Write(") && (");
					this.Visit(node.Right);
					this.Write(")");
					break;
				case ExpressionType.Assign:
					if (node.Left.Type.IsByRef && !node.Right.Type.IsByRef)
					{
						this.Write("*");
					}
					this.Visit(node.Left);
					this.Write(" = ");
					this.Visit(node.Right);
					break;
				case ExpressionType.Equal:
					this.Write("((");
					this.Visit(node.Left);
					this.Write(") == (");
					this.Visit(node.Right);
					this.Write("))");
					break;
				case ExpressionType.NotEqual:
					this.Write("((");
					this.Visit(node.Left);
					this.Write(") != (");
					this.Visit(node.Right);
					this.Write("))");
					break;
			}

			return node;
		}

		protected override Expression VisitConditional(ConditionalExpression node)
		{
			if (node.Type == typeof(void))
			{
				this.Write("if (");
				this.Visit(node.Test);
				this.Write(") ");

				if (node.IfTrue.NodeType == ExpressionType.Block)
				{
					this.WriteLine();
				}

				this.Visit(node.IfTrue);

				if (node.IfFalse.NodeType != ExpressionType.Default)
				{
					this.WriteLine();
					this.Write("else ");

					if (node.IfFalse.NodeType == ExpressionType.Block)
					{
						this.WriteLine();
					}

					this.Visit(node.IfFalse);

					if (node.IfFalse.NodeType != ExpressionType.Conditional)
					{
						this.WriteLine();
					}
				}
				else
				{
					this.WriteLine();
				}
			}
			else
			{
				this.Write('(');
				this.Visit(node.Test);
				this.Write(") ? (");
				this.Visit(node.IfTrue);
				this.Write(") : (");
				this.Visit(node.IfFalse);
				this.Write(")");
			}

			return node;
		}

		protected override Expression VisitGoto(GotoExpression node)
		{
			if (node.Kind == GotoExpressionKind.Return)
			{
				if (node.Value == null)
				{
					this.WriteLine("return");
				}
				else
				{
					this.Write("return ");
					this.Visit(node.Value);
				}
			}
			else if (node.Kind == GotoExpressionKind.Continue)
			{
				this.Write("continue");
			}

			return node;
		}

		protected override Expression VisitIncludeStatementExpresson(IncludeExpression expression)
		{
			this.Write("#import \"");
			this.Write(expression.FileName);
			this.WriteLine("\"");

			return expression;
		}

		protected override Expression VisitReferencedTypeExpresson(ReferencedTypeExpression expression)
		{
			this.Write("@class ");
			this.Write(expression.ReferencedType.Name);
			this.WriteLine(';');

			return expression;
		}

		protected override Expression VisitStatementExpression(StatementExpression expression)
		{
			this.Visit(expression.Expression);
			this.WriteLine(';');
			
			return expression;
		}

		protected override Expression VisitTypeDefinitionExpression(TypeDefinitionExpression expression)
		{
			if (expression.Header != null)
			{
				this.Visit(expression.Header);
				this.WriteLine();
			}

			var dryType = expression.Type as FickleType;

			if (dryType != null && dryType.IsClass)
			{
				if (!expression.IsPredeclaration)
				{
					this.WriteLine("#pragma clang diagnostic push");
					this.WriteLine("#pragma clang diagnostic ignored \"-Wparentheses\"");
					this.WriteLine();
				}

				if (expression.IsPredeclaration)
				{
					this.Write("@interface ");
					this.Write(expression.Type, true);
					this.Write(" : ");
					this.Write(expression.Type.BaseType, true);

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
					this.Write(expression.Type, true);
				}

				this.WriteLine();

				this.Visit(expression.Body);

				this.WriteLine("@end");
				this.WriteLine();

				if (!expression.IsPredeclaration)
				{
					this.WriteLine("#pragma clang diagnostic pop");
				}
			}
			else if (dryType != null && dryType.BaseType == typeof(Enum))
			{
				this.WriteLine("typedef enum");
				
				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					var expressions = ((GroupedExpressionsExpression)expression.Body).Expressions;

					var i = 0;

					foreach (BinaryExpression assignment in expressions)
					{
						this.Write(((ParameterExpression)assignment.Left).Name);
						this.Write(" = ");
						this.Visit(assignment.Right);

						if (i++ != expressions.Count - 1)
						{
							this.Write(',');
						}

						this.WriteLine();
					}
				}

				this.Write(expression.Type, true);
				this.WriteLine(";");
			}

			return expression;
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
			if ((method.AccessModifiers & AccessModifiers.ClasseslessFunction) != 0)
			{
				if ((method.AccessModifiers & AccessModifiers.Static) != 0)
				{
					this.Write("static ");
				}

				if (method.RawAttributes != null)
				{
					this.Write(method.RawAttributes);
					this.Write(' ');
				}

				this.Write(method.ReturnType);
				this.Write(" ");
				this.Write(method.Name);
				this.Write("(");
				for (var i = 0; i < method.Parameters.Count; i++)
				{
					var parameter = (ParameterExpression)method.Parameters[i];

					this.Write(parameter.Type);
					this.Write(" ");
					this.Write(parameter.Name);

					if (i != method.Parameters.Count - 1)
					{
						this.Write(", ");
					}
				}
				this.Write(")");

				if (method.IsPredeclaration)
				{
					this.Write(";");

					return method;
				}

				this.WriteLine();
				this.Visit(method.Body);
				this.WriteLine();
			}
			else
			{
				if ((method.AccessModifiers & AccessModifiers.Static) != 0)
				{
					this.Write("+");
				}
				else
				{
					this.Write("-");
				}

				this.Write('(');
				this.Write(method.ReturnType);
				this.Write(')');
				this.WriteSpace();
				this.Write(method.Name);

				for (var i = 0; i < method.Parameters.Count; i++)
				{
					var parameter = (ParameterExpression)method.Parameters[i];

					if (i != 0)
					{
						this.Write(parameter.Name);
					}

					this.Write(':');
					this.Write('(');
					this.Write(parameter.Type);
					this.Write(')');

					this.Write(parameter.Name);

					if (i != method.Parameters.Count - 1)
					{
						this.Write(" ");
					}
				}

				if (!string.IsNullOrEmpty(method.RawAttributes) && method.IsPredeclaration)
				{
					this.Write(" __attribute__({0})", method.RawAttributes);
				}

				if (method.IsPredeclaration)
				{
					this.WriteLine(';');
				}
				else
				{
					this.WriteLine();
					this.Visit(method.Body);
					this.WriteLine();
				}
			}

			return method;
		}

		protected override Expression VisitSimpleLambdaExpression(SimpleLambdaExpression node)
		{
			this.Write("^");

			if (node.ReturnType != null)
			{
				this.Write(node.ReturnType);
			}

			this.Write("(");

			foreach (ParameterExpression parameter in node.Parameters)
			{
				this.Write(parameter.Type);
				this.Write(" ");
				this.Write(parameter.Name);
			}

			this.WriteLine(")");

			if (node.Body.NodeType != ExpressionType.Block)
			{
				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
				{
					if (node.Variables.Count > 0)
					{	
						foreach (var variable in node.Variables)
						{
							this.WriteVariableDeclaration((ParameterExpression)variable);
						}

						this.WriteLine();
					}

					this.Visit(node.Body);
				}
			}
			else
			{
				this.Visit(node.Body);
			}

			return node;
		}

		protected override SwitchCase VisitSwitchCase(SwitchCase node)
		{
			foreach (var value in node.TestValues)
			{
				this.Write("case ");
				this.Visit(value);
				this.WriteLine(":");
			}

			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.Default))
			{
				this.Visit(node.Body);
				this.WriteLine("break;");
			}

			return node;
		}

		protected override Expression VisitSwitch(SwitchExpression node)
		{
			if (node.SwitchValue.Type.GetUnwrappedNullableType().IsIntegerType()
				|| node.SwitchValue.Type.GetUnwrappedNullableType().BaseType == typeof(Enum))
			{
				this.Write("switch");
				this.Write(" (");
				this.Visit(node.SwitchValue);
				this.WriteLine(")");

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					foreach (var switchCase in node.Cases)
					{
						this.VisitSwitchCase(switchCase);
					}

					this.WriteLine("default:");

					using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.Default))
					{
						this.Visit(node.DefaultBody);
					}
				}

				return node;
			}
			else
			{
				var j = 0;

				foreach (var switchCase in node.Cases)
				{
					if (j++ > 0)
					{
						this.Write("else ");
					}

					this.Write("if (");

					var i = 0;

					foreach (var testValue in switchCase.TestValues)
					{
						this.Write("([");
						this.Visit(node.SwitchValue);
						this.Write(" caseInsensitiveCompare:");
						this.Visit(testValue);
						this.Write("] == NSOrderedSame)");

						if (i++ < switchCase.TestValues.Count - 1)
						{
							this.Write(" || ");
						}
					}

					this.WriteLine(")");

					if (switchCase.Body.NodeType != ExpressionType.Block)
					{
						using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
						{
							this.Visit(switchCase.Body);
						}

						this.WriteLine();
					}
					else
					{
						this.Visit(switchCase.Body);
						this.WriteLine();
					}
				}

				if (node.DefaultBody != null)
				{
					if (node.Cases.Count > 0)
					{
						this.WriteLine("else");
					}

					using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
					{
						this.Visit(node.DefaultBody);	
					}
				}

				return node;
			}
		}

		protected override Expression VisitListInit(ListInitExpression node)
		{
			this.Write("@[");

			var i = 0;
			foreach (var value in node.Initializers)
			{
				this.Visit(value.Arguments[0]);

				if (i != node.Initializers.Count - 1)
				{
					this.Write(", ");
				}
			}

			this.Write("]");
			return node;
		}
	}
}
