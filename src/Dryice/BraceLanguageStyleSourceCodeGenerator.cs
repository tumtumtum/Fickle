//
// Copyright (c) 2013-2014 Thong Nguyen (tumtumtum@gmail.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dryice.Expressions;

namespace Dryice
{
	public class BraceLanguageStyleSourceCodeGenerator
		: SourceCodeGenerator
	{
		protected class CStyleIndentationContext
			: IDisposable
		{
			private readonly SourceCodeGenerator generator;
			private readonly BraceLanguageStyleIndentationOptions options;

			public CStyleIndentationContext(SourceCodeGenerator generator, BraceLanguageStyleIndentationOptions options)
			{
				this.generator = generator;
				this.options = options;

				if (options == BraceLanguageStyleIndentationOptions.IncludeBraces)
				{
					generator.WriteLine("{");
				}

				this.generator.CurrentIndent++;
			}

			public virtual void Dispose()
			{
				this.generator.CurrentIndent--;

				if (options == BraceLanguageStyleIndentationOptions.IncludeBraces)
				{
					generator.WriteLine("}");
				}
			}
		}

		public BraceLanguageStyleSourceCodeGenerator(TextWriter writer)
			: base(writer)
		{
		}

		protected override Expression VisitCommentExpression(CommentExpression expression)
		{
			this.WriteLine("// " + expression.Comment);

			return expression;
		}

		public override IDisposable AcquireIndentationContext()
		{
			return this.AcquireIndentationContext(BraceLanguageStyleIndentationOptions.IncludeBraces);
		}

		public virtual IDisposable AcquireIndentationContext(BraceLanguageStyleIndentationOptions options)
		{
			return new CStyleIndentationContext(this, options);
		}
	}
}
