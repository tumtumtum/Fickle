using System;
using System.IO;
using System.Linq.Expressions;
using Fickle.Expressions;
using Platform;

namespace Fickle.Generators.CSharp
{
	[PrimitiveTypeName(typeof(bool), "bool", false)]
	[PrimitiveTypeName(typeof(bool?), "bool?", true)]
	[PrimitiveTypeName(typeof(byte), "byte", false)]
	[PrimitiveTypeName(typeof(byte?), "byte?", true)]
	[PrimitiveTypeName(typeof(char), "char", false)]
	[PrimitiveTypeName(typeof(char?), "char?", true)]
	[PrimitiveTypeName(typeof(short), "short", false)]
	[PrimitiveTypeName(typeof(short?), "short?", true)]
	[PrimitiveTypeName(typeof(int), "int", false)]
	[PrimitiveTypeName(typeof(int?), "int?", true)]
	[PrimitiveTypeName(typeof(long), "long", false)]
	[PrimitiveTypeName(typeof(long?), "long?", true)]
	[PrimitiveTypeName(typeof(float), "float", false)]
	[PrimitiveTypeName(typeof(float?), "float?", true)]
	[PrimitiveTypeName(typeof(double), "double", false)]
	[PrimitiveTypeName(typeof(double?), "double?", true)]
	[PrimitiveTypeName(typeof(decimal), "decimal", false)]
	[PrimitiveTypeName(typeof(decimal?), "decimal?", true)]
	[PrimitiveTypeName(typeof(string), "string", true)]
	[PrimitiveTypeName(typeof(void), "void", false)]
	[PrimitiveTypeName(typeof(FickleListType), "List", true)]
	public class CSharpCodeGenerator
		: BraceLanguageStyleSourceCodeGenerator
	{
		public CSharpCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override void Write(Type type, bool nameOnly)
		{
			var nullableType = type as FickleNullable;

			if (nullableType != null)
			{
				var underlyingType = FickleNullable.GetUnderlyingType(type);
				this.Write(underlyingType, true);
				this.Write('?');

				return;
			}

			if (type.IsClass && type.Name == "Class")
			{
				this.Write("class");

				return;
			}

			if (type.IsInterface)
			{
				this.Write("interface ", type.Name);

				return;
			}

			var fickleListType = type as FickleListType;
			if (fickleListType != null)
			{
				this.Write("List");

				if (fickleListType.ListElementType != null)
				{
					this.Write("<");
					this.Write(fickleListType.ListElementType, true);
					this.Write(">");
				}

				return;
			}

			var fickleTaskType = type as CSharpAwaitedTaskType;
			if (fickleTaskType != null)
			{
				this.Write("Task");

				if (fickleTaskType.TaskElementType != null)
				{
					this.Write("<");
					this.Write(fickleTaskType.TaskElementType, true);
					this.Write(">");
				}

				return;
			}

			if (type == typeof(InterpolatedString))
			{
				this.Write("string");

				return;
			}

			var delegateType = type as FickleDelegateType;
			if (delegateType != null)
			{
				this.Write(delegateType.ReturnType, false);

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

				return;
			}

			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					this.Write(Nullable.GetUnderlyingType(type), true);
					this.Write('?');

					return;
				}

				this.Write(GetNameWithoutGenericArity(type) + "<");

				var genericTypeArguments = type.GenericTypeArguments;

				for (var i = 0; i < genericTypeArguments.Length; i++)
				{
					this.Write(genericTypeArguments[i], true);

					if (i + 1 < genericTypeArguments.Length)
					{
						this.Write(", ");
					}
				}

				this.Write(">");

				return;
			}

			base.Write(type, nameOnly);
		}

		private static string GetNameWithoutGenericArity(Type t)
		{
			var name = t.Name;
			var index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Convert)
			{
				if (node.Type == typeof(Guid) || node.Type == typeof(Guid?))
				{
					this.Write(typeof(Guid), true);
					this.Write(".Parse(");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(TimeSpan) || node.Type == typeof(TimeSpan?))
				{
					this.Write(typeof(TimeSpan), true);
					this.Write(".Parse(");
					this.Visit(node.Operand);
					this.Write(")");
				}
				else if (node.Type == typeof(object))
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
			else if (node.NodeType == ExpressionType.Throw)
			{
				this.Write("throw ");
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
			else if (node.Type == typeof (InterpolatedString))
			{
				var interpolatedString = ((InterpolatedString) node.Value).Value;
				this.Write("$\"" + interpolatedString + "\"");
			}
			else if (node.Type == typeof (Guid))
			{
				this.Write("Guid.Parse(\"" + Convert.ToString(node.Value) + "\")");
			}
			else if (node.Type == typeof (bool))
			{
				this.Write((bool) node.Value ? "true" : "false");
			}
			else
			{
				this.Write(Convert.ToString(node.Value));
			}

			return node;
		}

		protected override Expression VisitForEachExpression(ForEachExpression expression)
		{
			this.Write("foreach (var ");
			this.Visit(expression.VariableExpression);
			this.Write(" in ");
			this.Visit(expression.Target);
			this.Write(")");
			this.Visit(expression.Body);

			return expression;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
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

		protected override Expression VisitTypeBinary(TypeBinaryExpression node)
		{
			this.Visit(node.Expression);
			this.Write(" is ");
			this.Write(node.TypeOperand, true);

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
					this.Write(", ");
				}
			}

			this.Write(')');

			return node;
		}

		public override void ConvertToStringMethodCall(Expression expression)
		{
			if (expression.Type == typeof(string))
			{
				this.Visit(expression);
			}
			else
			{
				this.Visit(expression);
				this.Write(".ToString()");  
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.Name == SourceCodeGenerator.ToStringMethod)
			{
				this.ConvertToStringMethodCall(node.Object);

				return node;
			}

			if (node.Method.Name == SourceCodeGenerator.ToObjectMethod)
			{
				this.ConvertToObjectMethodCall(node);

				return node;
			}

			var fickleTaskReturnType = node.Method.ReturnType as CSharpAwaitedTaskType;
			if (fickleTaskReturnType != null)
			{
				this.Write("await ");
			}
				
			if (node.Object == null)
			{
				this.Write(node.Method.DeclaringType);
			}
			else
			{
				this.Visit(node.Object);
			}

			this.Write('.');
			this.Write(node.Method.Name);

			if (node.Method.IsGenericMethod)
			{
				var genericArgs = node.Method.GetGenericArguments();

				this.Write('<');

				for (var i = 0; i < genericArgs.Length; i++)
				{
					this.Write(genericArgs[i], true);

					if (i < genericArgs.Length - 1)
					{
						this.Write(", ");
					}
				}

				this.Write('>');
			}

			this.Write('(');

			for (var i = 0; i < node.Arguments.Count; i++)
			{
				var arg = node.Arguments[i];

				this.Visit(arg);

				if (i < node.Arguments.Count - 1)
				{
					this.Write(", ");
				}
			}

			this.Write(')');

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
					this.Write("(");
					this.Visit(node.Left);
					this.Write(") == (");
					this.Visit(node.Right);
					this.Write(")");
					break;
				case ExpressionType.NotEqual:
					this.Write("(");
					this.Visit(node.Left);
					this.Write(") != (");
					this.Visit(node.Right);
					this.Write(")");
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
			this.Write("using ");
			this.Write(expression.FileName);
			this.WriteLine(';');

			return expression;
		}

		protected override Expression VisitNamespaceExpresson(NamespaceExpression expression)
		{
			if (expression.Header != null)
			{
				this.Visit(expression.Header);
				this.WriteLine();
				this.WriteLine();
			}

			this.Write("namespace ");
			this.WriteLine(expression.NameSpace);
			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBracesNewLineAfter))
			{
				this.Visit(expression.Body);
				this.WriteLine();
			}

			return expression;
		}

		protected override Expression VisitReferencedTypeExpresson(ReferencedTypeExpression expression)
		{
			this.Write("using ");
			this.Write(expression.ReferencedType);
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
				this.WriteLine();
			}

			var fickleType = expression.Type as FickleType;

			if (fickleType != null && fickleType.IsClass)
			{
				this.Write("public class ");
				this.Write(expression.Type.Name, true);

				if (expression.Type.BaseType != null && expression.Type.BaseType != typeof(Object))
				{
					this.Write(" : ");
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
						i++;

						this.Visit(rootExpression);

						if (i < expressions.Count)
						{
							this.Write(',');
						}

						this.WriteLine();
					}
				}
			}

			return expression;
		}

		protected override Expression VisitPropertyDefinitionExpression(PropertyDefinitionExpression property)
		{
			this.Write(AccessModifiersToString(AccessModifiers.Public));
			this.Write(property.PropertyType);
			this.Write(' ');
			this.Write(property.PropertyName);
			this.WriteLine(" { get; set; }");

			return property;
		}

		protected override Expression VisitFieldDefinitionExpression(FieldDefinitionExpression field)
		{
			this.Write(AccessModifiersToString(field.AccessModifiers));
			this.Write(field.PropertyType);
			this.Write(' ');
			this.Write(field.PropertyName);
			this.Write(';');

			return field;
		}

		protected override Expression VisitMethodDefinitionExpression(MethodDefinitionExpression method)
		{
			this.WriteLine();

			this.Write(AccessModifiersToString(method.AccessModifiers));

			if (method.ReturnType != null) //null for constructors
			{
				var fickleTaskReturnType = method.ReturnType as CSharpAwaitedTaskType;
				if (fickleTaskReturnType != null)
				{
					this.Write("async ");
				}

				this.Write(method.ReturnType);
				this.Write(" ");
			}

			this.Write(method.Name);

			this.Write("(");
			for (var i = 0; i < method.Parameters.Count; i++)
			{
				var parameter = (ParameterExpression)method.Parameters[i];

				if (parameter.Type is FickleNullable)
				{
					this.Write(parameter.Type.GetUnwrappedNullableType());
				}
				else
				{
					this.Write(parameter.Type);	
				}
				
				this.Write(" ");
				this.Write(parameter.Name);

				if (i != method.Parameters.Count - 1)
				{
					this.Write(", ");
				}
			}

			this.Write(")");

			this.WriteLine();

			this.Visit(method.Body);

			return method;
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

		protected override Expression VisitTry(TryExpression node)
		{
			this.WriteLine("try");
			using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
			{
				this.Visit(node.Body);
			}

			foreach (var handler in node.Handlers)
			{
				this.WriteLine();
				this.Write("catch (");
				this.Write(handler.Test.Name);
				this.WriteLine(" exception)");

				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
				{
					this.Visit(handler.Body);
				}
			}

			if (node.Finally != null)
			{
				this.WriteLine();
				this.WriteLine("finally");
				using (this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces))
				{
					this.Visit(node.Finally);
				}
			}

			this.WriteLine();

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
						if (switchCase.TestValues.Count > 1)
						{
							this.Write("(");
						}
						this.Visit(node.SwitchValue);
						this.Write(" == ");
						this.Visit(testValue);
						if (switchCase.TestValues.Count > 1)
						{
							this.Write(")");
						}

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

		protected override Expression VisitWhileExpression(WhileExpression expression)
		{
			this.Write("while (");
			this.Visit(expression.Test);
			this.WriteLine(")");
			this.Visit(expression.Body);

			return expression;
		}

		protected override Expression VisitSimpleLambdaExpression(SimpleLambdaExpression node)
		{
			this.Write('(');

			for (var i = 0; i < node.Parameters.Count; i++)
			{
				var paramterExpression = node.Parameters[i] as ParameterExpression;

				if (paramterExpression == null)
				{
					continue;
				}

				this.Write(paramterExpression.Name);

				if (i < node.Parameters.Count - 1)
				{
					this.Write(", ");
				}
			}
			
			this.WriteLine(") => ");

			this.Visit(node.Body);

			return node;
		}

		private static String AccessModifiersToString(AccessModifiers accessModifiers)
		{
			var result = "";

			if ((accessModifiers & AccessModifiers.Public) != 0)
			{
				result += "public ";
			}
			else if ((accessModifiers & AccessModifiers.Private) != 0)
			{
				result += "private ";
			}
			else if ((accessModifiers & AccessModifiers.Protected) != 0)
			{
				result += "protected ";
			}

			if ((accessModifiers & AccessModifiers.Static) != 0)
			{
				result += "static ";
			}

			if ((accessModifiers & AccessModifiers.Constant) != 0)
			{
				result += "const ";
			}

			if ((accessModifiers & AccessModifiers.ReadOnly) != 0)
			{
				result += "readonly ";
			}

			return result;
		}
	}
}
