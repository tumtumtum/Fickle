using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dryice.Generators;
using Dryice.Model;
using Platform.Reflection;

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
				if (property.GetFirstCustomAttribute<ServiceAnnotationAttribute>(true) == null)
				{
					continue;
				}

				var value = property.GetValue(obj);

				if (value != null)
				{
					this.Write("@");
					this.Write(property.Name.ToLower());
					this.Write(" ");
					this.WriteLine(value.ToString().ToLower());
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
			this.WriteLine("class {0}", serviceClass.Name);

			using (this.AcquireIndentationContext())
			{
				if (serviceClass.BaseTypeName != null)
				{
					this.WriteLine("@extends " + serviceClass.BaseTypeName);
				}

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
			this.WriteLine("gateway {0}", serviceGateway.Name);

			using (this.AcquireIndentationContext())
			{
				this.WriteAnnotations(serviceGateway);

				foreach (var method in serviceGateway.Methods)
				{
					this.Write(method.Name);

					this.Write("(");

					var i = 0;

					foreach (var parameter in method.Parameters)
					{
						this.Write(parameter.Name);
						this.Write(":");
						this.Write(parameter.TypeName);

						if (i++ < method.Parameters.Count - 1)
						{
							this.Write(" ");
						}
					}

					this.WriteLine(")");

					using (this.AcquireIndentationContext())
					{
						this.WriteAnnotations(method);
					}
				}
			}
		}

		public virtual void Write(ServiceModel serviceModel)
		{
			this.ListAction(serviceModel.Enums, this.Write);
			
			if (serviceModel.Enums.Count > 0)
			{
				this.WriteLine();
			}

			this.ListAction(serviceModel.Classes, this.Write);

			if (serviceModel.Classes.Count > 0)
			{
				this.WriteLine();
			}

			this.ListAction(serviceModel.Gateways, this.Write);
		}
	}
}
