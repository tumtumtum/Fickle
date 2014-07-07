//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.IO;
using System.Reflection;
using Platform;
using System.Linq;
using System.Linq.Expressions;
using Dryice.Expressions;

namespace Dryice.Generators.Objective
{
	[PrimitiveTypeName(typeof(byte), "UInt8", false)]
	[PrimitiveTypeName(typeof(char), "unichar", false)]
	[PrimitiveTypeName(typeof(short), "Int16", false)]
	[PrimitiveTypeName(typeof(int), "Int", false)]
	[PrimitiveTypeName(typeof(int), "Int", false)]
	[PrimitiveTypeName(typeof(long), "Int64", false)]
	[PrimitiveTypeName(typeof(float), "Float32", false)]
	[PrimitiveTypeName(typeof(double), "Float64", false)]
	[PrimitiveTypeName(typeof(Guid), "PKUUID", true)]
	[PrimitiveTypeName(typeof(DateTime), "NSDate", true)]
	[PrimitiveTypeName(typeof(TimeSpan), "PKTimeSpan", true)]
	[PrimitiveTypeName(typeof(string), "NSString", true)]
	public class ObjectiveCodeGenerator
		: BraceLanguageStyleSourceCodeGenerator
	{
		public ObjectiveCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override void Write(Type type, bool nameOnly)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);

			if (type == typeof(object))
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
			else if (underlyingType != null && underlyingType.IsPrimitive)
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
			else if (type.IsInterface)
			{
				this.Write("id<{0}>", type.Name);

				return;
			}
			else if (type.GetDryiceListElementType() != null)
			{
				if (nameOnly)
				{
					this.Write("NSArray");
				}
				else
				{
					this.Write("NSArray*");
				}

				return;
			}

			var dryiceType = type as DryType;

			if (dryiceType != null)
			{
				base.Write(type, nameOnly);

				if (!nameOnly && dryiceType.IsClass)
				{
					base.Write("*");
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
			if (node.NodeType == ExpressionType.Convert)
			{
				if (node.Type == typeof(Guid) || node.Type == typeof(Guid?))
				{
					this.Write("[PKUUID uuidFromString:");
					this.Visit(node.Operand);
					this.Write("]");
				}
				else if (node.Type == typeof(byte))
				{
					this.Write("(uint8_t)((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").intValue");
				}
				else if (node.Type == typeof(byte?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(short))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").shortValue");
				}
				else if (node.Type == typeof(short?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(int))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").intValue");
				}
				else if (node.Type == typeof(int?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(long))
				{
					this.Write("(int64_t)((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").longLongValue");
				}
				else if (node.Type == typeof(long?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(float))
				{
					this.Write("(float)((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").floatValue");
				}
				else if (node.Type == typeof(float?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(double))
				{
					this.Write("(double)((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(").doubleValue");
				}
				else if (node.Type == typeof(double?))
				{
					this.Write("((NSNumber*)");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(DateTime?) || node.Type == typeof(DateTime))
				{
					this.Write("((NSString*)currentValueFromDictionary).length >= 16 ? [NSDate dateWithTimeIntervalSince1970:[[(NSString*)");
					this.Visit(node.Operand);
					this.Write("substringWithRange:NSMakeRange(6, 10)] intValue]] : nil");
				}
				else if (node.Type == typeof(TimeSpan?) || node.Type == typeof(TimeSpan))
				{
					this.Write("[TimeSpan fromIsoString:(NSString*)");
					this.Visit(node.Operand);
					this.Write("]");
				}
				else if (node.Type == typeof(object))
				{
					if (node.Operand.Type.IsPrimitive)
					{
						if (node.Operand.Type == typeof(long))
						{
							this.Write("[NSNumber numberWithLongLong:");
							this.Visit(node.Operand);
							this.Write("]");
						}
						else if (node.Operand.Type == typeof(float))
						{
							this.Write("[NSNumber numberWithFloat:");
							this.Visit(node.Operand);
							this.Write("]");
						}
						else if (node.Operand.Type == typeof(double))
						{
							this.Write("[NSNumber numberWithDouble:");
							this.Visit(node.Operand);
							this.Write("]");
						}
						else
						{
							this.Write("[NSNumber numberWithInt:");
							this.Visit(node.Operand);
							this.Write("]");
						}
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
					this.Visit(node.Operand);
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
				this.Write("\"" + Convert.ToString(node.Value) + "\"");
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
			this.Write(")");
			this.Visit(expression.Body);

			return expression;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (node.IsByRef)
			{
				this.Write('&');
			}

			this.Write(node.Name);

			return node;
		}

		protected virtual void WriteVariableDeclaration(ParameterExpression node)
		{
			this.Write(node.Type);	
			this.Write(' ');
			this.Write(node.Name);
			this.WriteLine(';');
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			this.WriteLine();

			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
			{
				if (node.Variables != null)
				{
					foreach (var expression in node.Variables)
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
					this.Visit(node.Arguments[1]);
				}
			}

			this.Write(']');

			return node;
		}

		public override void Generate(Expression expression)
		{
			base.Generate(DateFormatterInserter.Insert(expression));
		}

		public override void ConvertToString(Expression expression)
		{
			if (expression.Type == typeof(string))
			{
				this.Visit(expression);
			}
			else if (expression.Type == typeof(Guid) || expression.Type == typeof(Guid?))
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" ");
				this.Write(" compactStringValue");
				this.Write("]");
			}
			else if (expression.Type == typeof(TimeSpan) || expression.Type == typeof(TimeSpan?))
			{
				this.Write("[");
				this.Visit(expression);
				this.Write(" ");
				this.Write(" toIsoString");
				this.Write("]");
			}
			else if (expression.Type == typeof(DateTime) || expression.Type == typeof(DateTime?))
			{
				this.Write("[dateFormatter stringFromDate:");
				this.Visit(expression);
				this.Write("]");
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.Name == "ToString")
			{
				ConvertToString(node.Object);

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
				case ExpressionType.Assign:
					this.Visit(node.Left);
					this.Write(" = ");
					this.Visit(node.Right);
					break;
				case ExpressionType.Equal:
					this.Write('(');
					this.Visit(node.Left);
					this.Write(" == ");
					this.Visit(node.Right);
					this.Write(')');
					break;
				case ExpressionType.NotEqual:
					this.Write('(');
					this.Visit(node.Left);
					this.Write(" != ");
					this.Visit(node.Right);
					this.Write(')');
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

				this.Visit(node.IfTrue);

				if (node.IfFalse.NodeType != ExpressionType.Default)
				{
					this.WriteLine("else ");
					
					this.Visit(node.IfFalse);
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

			return node;
		}

		protected override Expression VisitIncludeStatementExpresson(IncludeStatementExpression expression)
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
			this.Visit(expression.Header);

			this.WriteLine();

			if (expression.IsPredeclaration)
			{
				this.Write("@interface ");
				this.Write(expression.Name);
				this.Write(" : ");
				this.Write(expression.BaseType, true);

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

			this.Visit(expression.Body);

			this.WriteLine("@end");

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
			if (method.IsStatic)
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

			if (method.Parameters != null)
			{
				this.Write(':');

				for (var i = 0; i < method.Parameters.Count; i++)
				{
					var parameter = (ParameterExpression)method.Parameters[i];

					if (i != 0)
					{
						this.Write(parameter.Name);

						this.Write('(');
						this.Write(parameter.Type);
						this.Write(')');
						this.Write(':');
					}

					this.Write(parameter.Name);

					if (i != method.Parameters.Count - 1)
					{
						this.Write(" ");
					}
				}
			}

			if (!string.IsNullOrEmpty(method.RawAttributes) && method.IsPredeclatation)
			{
				this.Write(" __attribute__({0})", method.RawAttributes);
			}

			if (method.IsPredeclatation)
			{
				this.WriteLine(';');
			}
			else
			{
				this.Visit(method.Body);
			}

			return method;
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return base.VisitLambda<T>(node);
		}
	}
}
