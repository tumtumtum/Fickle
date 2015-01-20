using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Fickle.Generators;
using Fickle.Model;
using Platform.Reflection;

namespace Fickle.Ficklefile
{
	public class FicklefileWriter
		: SourceCodeGenerator
	{
		private readonly HashSet<string> keywords = new HashSet<string>(typeof(FicklefileKeyword).GetEnumNames(), StringComparer.InvariantCultureIgnoreCase);

		public FicklefileWriter(TextWriter writer)
			: base(writer)
		{
		}

		protected virtual void WriteIdentifier(string name)
		{
			if (keywords.Contains(name))
			{
				this.Write("$");
				this.Write(name);
			}
			else
			{
				this.Write(name);
			}
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

					if (value is bool)
					{
						this.WriteLine(value.ToString().ToLower());
					}
					else
					{
						this.WriteLine(value.ToString());
					}
				}
			}
		}

		protected virtual void Write(ServiceEnum serviceEnum)
		{
			this.Write("enum ");
			this.WriteIdentifier(serviceEnum.Name);
			this.WriteLine();

			using (this.AcquireIndentationContext())
			{
				foreach (var value in serviceEnum.Values)
				{
					this.WriteLine("{0}:{1}", value.Name, value.Value);
				}
			}
		}

		protected virtual void Write(ServiceClass serviceClass)
		{
			this.Write("class ");
			this.WriteIdentifier(serviceClass.Name);
			this.WriteLine();

			using (this.AcquireIndentationContext())
			{
				if (serviceClass.BaseTypeName != null)
				{
					this.WriteLine("@extends " + serviceClass.BaseTypeName);
				}

				foreach (var value in serviceClass.Properties)
				{
					this.WriteIdentifier(value.Name);
					this.Write(":");
					this.WriteIdentifier(value.TypeName);
					this.WriteLine();
				}
			}
		}

		protected virtual void Write(ServiceMethod serviceMethod)
		{
			this.WriteIdentifier(serviceMethod.Name);
			this.Write("(");

			var i = 0;

			foreach (var parameter in serviceMethod.Parameters)
			{
				this.WriteIdentifier(parameter.Name);
				this.Write(":");
				this.WriteIdentifier(parameter.TypeName);

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
			this.Write("gateway ");
			this.WriteIdentifier(serviceGateway.Name);

			using (this.AcquireIndentationContext())
			{
				this.WriteAnnotations(serviceGateway);

				foreach (var method in serviceGateway.Methods)
				{
					this.WriteIdentifier(method.Name);

					this.Write("(");

					var i = 0;

					foreach (var parameter in method.Parameters)
					{
						this.WriteIdentifier(parameter.Name);
						this.Write(":");
						this.WriteIdentifier(parameter.TypeName);

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
			
			this.ListAction(serviceModel.Classes, this.Write);

			this.ListAction(serviceModel.Gateways, this.Write);
		}
	}
}
