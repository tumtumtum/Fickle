//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Fickle.Expressions;

namespace Fickle.Generators
{
	public abstract class SourceCodeGenerator
		: ServiceExpressionVisitor
	{
		public static String ToStringMethod = "ToString";
		public static String ToObjectMethod = "ToObject";

		public int CurrentIndent { get; protected internal set; }

		private bool indentRequired;
		private readonly TextWriter writer;

		private Dictionary<Type, string> typeNameByPrimitiveType;
		private Dictionary<Type, bool> isReferenceTypeByType;

		protected class IndentationContext : IDisposable
		{
			private readonly SourceCodeGenerator generator;

			public IndentationContext(SourceCodeGenerator generator)
			{
				this.generator = generator;

				generator.CurrentIndent++;
			}

			public virtual void Dispose()
			{
				this.generator.CurrentIndent--;
			}
		}

		protected SourceCodeGenerator(TextWriter writer)
		{
			this.writer = writer;
		}

		public virtual void Generate(Expression expression)
		{
			this.Visit(expression);
		}

		protected virtual void WriteSpace()
		{
			this.Write(" ");
		}

		protected virtual void AddPrimitiveTypeDecl(Type type, string name, bool isReferenceType)
		{
			this.typeNameByPrimitiveType[type] = name;
			this.isReferenceTypeByType[type] = isReferenceType;
		}

		protected virtual bool IsReferenceType(Type type)
		{
			bool value;

			if (this.isReferenceTypeByType.TryGetValue(type, out value))
			{
				return value;
			}

			if (type.IsNullable())
			{
				return true;
			}

			return !TypeSystem.IsPrimitiveType(type);
		}
		
		protected virtual void Write(Type type)
		{
			this.Write(type, false);
		}

		protected virtual void Write(Type type, bool nameOnly)
		{
			if (this.typeNameByPrimitiveType == null)
			{
				this.typeNameByPrimitiveType = new Dictionary<Type, string>();
				this.isReferenceTypeByType = new Dictionary<Type, bool>();

				foreach (PrimitiveTypeNameAttribute attribute in this.GetType().GetCustomAttributes(typeof(PrimitiveTypeNameAttribute), true))
				{
					this.AddPrimitiveTypeDecl(attribute.Type, attribute.Name, attribute.IsReferenceType);
				}
			}

			string name;

			if (this.typeNameByPrimitiveType.TryGetValue(type, out name))
			{
				this.Write(name);

				return;
			}

			this.Write(type.Name);
		}

		protected virtual void WriteIndent()
		{
			for (var i = 0; i < this.CurrentIndent; i++)
			{
				this.writer.Write('\t');
			}
		}

		private void CheckAndWriteIndent()
		{
			if (this.indentRequired)
			{
				this.WriteIndent();
				this.indentRequired = false;
			}
		}

		public virtual void Write(char value)
		{
			this.CheckAndWriteIndent();

			this.writer.Write(value);
		}

		public virtual void Write(string value)
		{
			this.CheckAndWriteIndent();

			this.writer.Write(value);
		}

		public virtual void Write(string format, params object[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}

			this.CheckAndWriteIndent();

			this.writer.Write(format, args);
		}

		public virtual void WriteLine()
		{
			this.CheckAndWriteIndent();

			this.writer.WriteLine();

			this.indentRequired = true;
		}

		public virtual void WriteLine(char c)
		{
			this.CheckAndWriteIndent();

			this.writer.Write(c);
			this.WriteLine();

			this.indentRequired = true;
		}

		public virtual void WriteLine(string value)
		{
			this.CheckAndWriteIndent();

			this.writer.WriteLine(value);

			this.indentRequired = true;
		}

		public virtual void WriteLine(string format, params object[] args)
		{
			this.CheckAndWriteIndent();

			this.writer.WriteLine(format, args);

			this.indentRequired = true;
		}

		protected override Expression VisitGroupedExpressionsExpression(GroupedExpressionsExpression groupedExpression)
		{
			var x = 0;
			var original = groupedExpression.Expressions;
			
			foreach (var expression in original)
			{
				var isLast = x == original.Count - 1;

				this.Visit(expression);

				if (expression != null && groupedExpression.Style == GroupedExpressionsExpressionStyle.Wide && !isLast)
				{
					this.WriteLine();
				}

				x++;
			}

			return groupedExpression;
		}

		public virtual IDisposable AcquireIndentationContext()
		{
			return new IndentationContext(this);
		}

		public virtual void ConvertToStringMethodCall(Expression expression)
		{
		}

		public virtual void ConvertToObjectMethodCall(Expression expression)
		{
		}

		protected override Expression VisitCodeLiteralExpression(CodeLiteralExpression expression)
		{
			expression.Action(this);

			return expression;
		}

		protected virtual void ListAction<T>(IList<T> items, Action<T> action, bool insertLinesInbetween = true)
		{
			var i = 0;

			foreach (var item in items)
			{
				action(item);

				if (insertLinesInbetween && i++ < items.Count)
				{
					this.WriteLine();
				}
			}
		}

		protected String GetTypeNameFromPrimitiveType(Type primitiveType)
		{
			if (!typeNameByPrimitiveType.Keys.Contains(primitiveType))
			{
				return primitiveType.ToString();
			}

			return typeNameByPrimitiveType[primitiveType];
		}
	}
}
