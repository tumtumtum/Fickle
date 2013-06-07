//
// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)
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

		private Dictionary<PrimitiveType, string> typeNameByPrimitiveType;

		protected virtual void AddPrimitiveTypeName(PrimitiveType primitiveType, string name)
		{
			typeNameByPrimitiveType[primitiveType] = name;
		}

		protected virtual void Write(ServiceType serviceType)
		{
			if (serviceType.IsPrimitive)
			{
				if (typeNameByPrimitiveType == null)
				{
					typeNameByPrimitiveType = new Dictionary<PrimitiveType, string>();

					foreach (PrimitiveTypeNameAttribute attribute in this.GetType().GetCustomAttributes(typeof(PrimitiveTypeNameAttribute), true))
					{
						this.AddPrimitiveTypeName(attribute.PrimitiveType, attribute.Name);
					}
				}

				PrimitiveType value;

				if (!Enum.TryParse(serviceType.Name, out value))
				{
					throw new InvalidOperationException("Unsupported type: " + serviceType.Name);
				}

				string name;

				if (typeNameByPrimitiveType.TryGetValue(value, out name))
				{
					this.Write(name);

					return;
				}
			}

			this.Write(serviceType.Name);
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
			this.writer.WriteLine();

			indentRequired = true;
		}

		public virtual void WriteLine(char c)
		{
			this.writer.WriteLine(c);

			indentRequired = true;
		}

		public virtual void WriteLine(string value)
		{
			this.writer.WriteLine(value);

			indentRequired = true;
		}

		public virtual void WriteLine(string format, params object[] args)
		{
			this.writer.WriteLine(format, args);

			indentRequired = true;
		}

		protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			int x = 0;

			foreach (var expression in original)
			{
				var isLast = x == original.Count - 1;

				this.Visit(expression);

				if (expression.NodeType == (ExpressionType)ServiceExpressionType.GroupedExpressions
					&& ((GroupedExpressionsExpression)expression).Isolated
					&& !isLast)
				{
					this.writer.WriteLine();
				}

				x++;
			}

			return original;
		}

		public virtual IDisposable AcquireIndentationContext()
		{
			return new IndentationContext(this);
		}
	}
}
