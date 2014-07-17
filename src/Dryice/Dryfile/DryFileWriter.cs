using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dryice.Generators;
using Dryice.Model;

namespace Dryice.Dryfile
{
	public class DryFileWriter
		: SourceCodeGenerator
	{
		public DryFileWriter(TextWriter writer)
			: base(writer)
		{
		}

		public virtual void Generate(ServiceModel serviceModel)
		{
			this.Write(serviceModel);
		}

		protected virtual void WriteAnnotations<T>(T obj)
		{
			var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var value = property.GetValue(obj);

				if (value != null)
				{
					this.Write("@");
					this.Write(property.Name.ToLower());
					this.Write(" ");
					this.Write(value.ToString().ToLower());
				}
			}
		}

		protected virtual void Write(ServiceEnum serviceEnum)
		{
			this.Write("enum {0}", serviceEnum.Name);

			using (this.AcquireIndentationContext())
			{
				foreach (var value in serviceEnum.Values)
				{
					this.WriteLine("{0} : {1}", value.Name, value.Value);
				}
			}
		}

		protected virtual void Write(ServiceClass serviceClass)
		{
			this.Write("class {0}", serviceClass.Name);

			using (this.AcquireIndentationContext())
			{
				foreach (var value in serviceClass.Properties)
				{
					this.WriteLine("{0} : {1}", value.Name, value.TypeName);
				}
			}
		}

		protected virtual void Write(ServiceMethod serviceMethod)
		{
			this.Write("{0}(");

			var i = 0;

			foreach (var parameter in serviceMethod.Parameters)
			{
				this.Write(parameter.Name);
				this.Write(":");
				this.Write(parameter.TypeName);

				if (i++ < serviceMethod.Parameters.Count - 1)
				{
					this.Write(" ");
				}
			}

			this.Write(")");

			this.WriteLine();

			using (this.AcquireIndentationContext())
			{
				this.WriteAnnotations(serviceMethod);
			}
		}

		protected virtual void Write(ServiceGateway serviceGateway)
		{
			this.Write("gateway {0}", serviceGateway.Name);

			using (this.AcquireIndentationContext())
			{
				this.WriteAnnotations(serviceGateway);
			}
		}

		public virtual void Write(ServiceModel serviceModel)
		{
			foreach (var serviceEnum in serviceModel.Enums)
			{
				this.Write(serviceEnum);
				this.WriteLine();
			}

			foreach (var serviceClass in serviceModel.Classes)
			{
				this.Write(serviceClass);
				this.WriteLine();
			}

			foreach (var serviceGateway in serviceModel.Gateways)
			{
				this.Write(serviceGateway);
				this.WriteLine();
			}
		}
	}
}
