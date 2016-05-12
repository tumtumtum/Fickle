using System;
using System.IO;
using Platform;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators.Javascript
{
	[PrimitiveTypeName(typeof(bool), "boolean", false)]
	[PrimitiveTypeName(typeof(bool?), "Boolean", true)]
	[PrimitiveTypeName(typeof(byte), "byte", false)]
	[PrimitiveTypeName(typeof(byte?), "Byte", true)]
	[PrimitiveTypeName(typeof(char), "char", false)]
	[PrimitiveTypeName(typeof(char?), "Character", true)]
	[PrimitiveTypeName(typeof(short), "short", false)]
	[PrimitiveTypeName(typeof(short?), "Short", true)]
	[PrimitiveTypeName(typeof(int), "int", false)]
	[PrimitiveTypeName(typeof(int?), "Integer", true)]
	[PrimitiveTypeName(typeof(long), "long", false)]
	[PrimitiveTypeName(typeof(long?), "Long", true)]
	[PrimitiveTypeName(typeof(float), "float", false)]
	[PrimitiveTypeName(typeof(float?), "Float", true)]
	[PrimitiveTypeName(typeof(double), "double", false)]
	[PrimitiveTypeName(typeof(double?), "Double", true)]
	[PrimitiveTypeName(typeof(Guid), "UUID", true)]
	[PrimitiveTypeName(typeof(Guid?), "UUID", true)]
	[PrimitiveTypeName(typeof(DateTime), "Date", true)]
	[PrimitiveTypeName(typeof(DateTime?), "Date", true)]
	[PrimitiveTypeName(typeof(TimeSpan), "TimeSpan", true)]
	[PrimitiveTypeName(typeof(TimeSpan?), "TimeSpan", true)]
	[PrimitiveTypeName(typeof(Exception), "Exception", true)]
	[PrimitiveTypeName(typeof(string), "String", true)]
	[PrimitiveTypeName(typeof(FickleListType), "ArrayList", true)]
	public class JavascriptCodeGenerator
		: BraceLanguageStyleSourceCodeGenerator
	{
		private readonly string[] reservedKeywords = new string[]
		{
			"var",
			"function",
			"this",
			"return"
		};

		public JavascriptCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override void Write(Type type, bool nameOnly)
		{
			var underlyingType = FickleNullable.GetUnderlyingType(type);

			if (underlyingType != null && underlyingType.BaseType == typeof(Enum))
			{
				this.WriteLine(type.Name);

				return;
			}
			else if (type == typeof(object))
			{
				this.Write("var");

				return;
			}
			else if (type is FickleListType)
			{
				this.Write("Array");

				return;
			}

			base.Write(type, nameOnly);
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Convert)
			{
				if (node.Type == typeof(object))
				{
					this.Visit(node.Operand);
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
				this.Write("null");

				return node;
			}

			if (node.Type == typeof(string))
			{
				this.Write("\"" + Convert.ToString(node.Value) + "\"");
			}
			else if (node.Type == typeof(Guid))
			{
				this.Write("UUID.fromString(\"" + Convert.ToString(node.Value) + "\")");
			}
			else if (node.Type == typeof(bool))
			{
				this.Write((bool)node.Value ? "true" : "false");
			}
			else
			{
				this.Write(Convert.ToString(node.Value));
			}

			return node;
		}
		
		protected override Expression VisitParameter(ParameterExpression node)
		{
			this.Write(node.Name);

			return node;
		}

		protected virtual void WriteVariableDeclaration(ParameterExpression node)
		{
			this.Write("var ");
			this.Write(node.Name);
			this.WriteLine(';');
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
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
				this.Write(node.Member.Name);
			}

			return node;
		}
		
		protected override Expression VisitNew(NewExpression node)
		{
			this.Write("new ");
			this.Write(node.Type, true);
			this.Write("(");

			for (int i = 0; i < node.Arguments.Count; i++)
			{
				var expression = node.Arguments[i];
				this.Visit(expression);

				if (i + 1 != node.Arguments.Count)
				{
					Write(", ");
				}
			}

			this.Write(')');

			return node;
		}

		public override void ConvertToStringMethodCall(Expression expression)
		{
			this.Visit(expression);
		}

		public override void ConvertToObjectMethodCall(Expression expression)
		{
			var methodCallArgument = ((MethodCallExpression) expression).Arguments[0];

			this.Visit(methodCallArgument); 
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
				this.WriteLine();
			}

			var fickleType = expression.Type as FickleType;

			if (fickleType != null && fickleType.IsClass)
			{
				this.Write("public class ");
				this.Write(expression.Type.Name, true);

				if (expression.Type.BaseType != null && expression.Type.BaseType != typeof(Object))
				{
					this.Write(" extends ");
					this.Write(expression.Type.BaseType, true);
				}

				this.WriteLine();

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					this.Visit(expression.Body);
					this.WriteLine();
				}
			}
			else if (fickleType != null && fickleType.BaseType == typeof(Enum))
			{
				this.WriteLine("public enum " + expression.Type.Name);

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
				{
					var expressions = ((GroupedExpressionsExpression)expression.Body).Expressions;

					var i = 0;

					foreach (Expression rootExpression in expressions)
					{
						if (rootExpression is GroupedExpressionsExpression)
						{
							var binaryExpressions = ((GroupedExpressionsExpression) rootExpression).Expressions;

							foreach (Expression binaryExpression in binaryExpressions)
							{
								var assignment = (BinaryExpression)binaryExpression;

								this.Write(((ParameterExpression) assignment.Left).Name);
								this.Write("(");
								this.Visit(assignment.Right);
								this.Write(")");

								if (i++ != binaryExpressions.Count - 1)
								{
									this.WriteLine(',');
								}
								else
								{
									this.WriteLine(';');
								}
							}
						}
						else 
						{
							this.Visit(rootExpression);
						}

						this.WriteLine();
					}
				}
			}

			return expression;
		}

		protected override Expression VisitFieldDefinitionExpression(FieldDefinitionExpression field)
		{
			this.Write(field.PropertyType);
			this.Write(' ');
			this.Write(field.PropertyName);
			this.Write(';');

			return field;
		}
		
	}
}
