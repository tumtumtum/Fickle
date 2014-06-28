//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using Sublimate.Expressions;
using Sublimate.Model;

namespace Sublimate
{
	public abstract class SourceCodeGenerator
		: ServiceExpressionVisitor
	{
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
				generator.CurrentIndent--;
			}
		}

		public int CurrentIndent { get; protected internal set; }

		private bool indentRequired;
		private readonly TextWriter writer;

		public SourceCodeGenerator(TextWriter writer)
		{
			this.writer = writer;
		}

		public void Generate(Expression expression)
		{
			this.Visit(expression);
		}

		protected virtual void WriteSpace()
		{
			this.Write(" ");
		}

		private Dictionary<Type, string> typeNameByPrimitiveType;
		private Dictionary<Type, bool> isReferenceTypeByType;

		protected virtual void AddPrimitiveTypeDecl(Type type, string name, bool isReferenceType)
		{
			typeNameByPrimitiveType[type] = name;
			isReferenceTypeByType[type] = isReferenceType;
		}

		protected virtual bool IsReferenceType(Type type)
		{
			bool value;

			if (isReferenceTypeByType.TryGetValue(type, out value))
			{
				return value;
			}

			return true;
		}
		
		protected virtual void Write(Type type)
		{
			this.Write(type, false);
		}

		protected virtual void Write(Type type, bool nameOnly)
		{
			if (typeNameByPrimitiveType == null)
			{
				typeNameByPrimitiveType = new Dictionary<Type, string>();
				isReferenceTypeByType = new Dictionary<Type, bool>();

				foreach (PrimitiveTypeNameAttribute attribute in this.GetType().GetCustomAttributes(typeof(PrimitiveTypeNameAttribute), true))
				{
					this.AddPrimitiveTypeDecl(attribute.Type, attribute.Name, attribute.IsReferenceType);
				}
			}

			string name;

			if (typeNameByPrimitiveType.TryGetValue(type, out name))
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
			if (indentRequired)
			{
				this.WriteIndent();
				indentRequired = false;
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

			indentRequired = true;
		}

		public virtual void WriteLine(char c)
		{
			this.CheckAndWriteIndent();

			this.writer.Write(c);
			this.WriteLine();

			indentRequired = true;
		}

		public virtual void WriteLine(string value)
		{
			this.CheckAndWriteIndent();

			this.writer.WriteLine(value);

			indentRequired = true;
		}

		public virtual void WriteLine(string format, params object[] args)
		{
			this.CheckAndWriteIndent();

			this.writer.WriteLine(format, args);

			indentRequired = true;
		}

		protected override Expression VisitGroupedExpressionsExpression(GroupedExpressionsExpression groupedExpression)
		{
			var x = 0;
			var original = groupedExpression.Expressions;
			
			foreach (var expression in original)
			{
				var isLast = x == original.Count - 1;

				this.Visit(expression);

				if (groupedExpression.Style == GroupedExpressionsExpressionStyle.Wide && !isLast)
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
	}
}
