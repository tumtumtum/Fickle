using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dryice
{
	public  class DryConstructorInfo
		: ConstructorInfo
	{
		private readonly string name;
		private readonly Type declaringType;
		private readonly ParameterInfo[] parameters;

		public DryConstructorInfo(Type declaringType, string name, ParameterInfo[] parameters)
		{
			this.name = name;
			this.declaringType = declaringType;
			this.parameters = parameters;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override ParameterInfo[] GetParameters()
		{
			return this.parameters;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotImplementedException();
		}

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override string Name
		{
			get
			{
				return this.name;
			}
		}

		public override Type DeclaringType
		{
			get
			{
				return declaringType;
			}
		}

		public override Type ReflectedType
		{
			get
			{
				return declaringType;
			}
		}

		public override RuntimeMethodHandle MethodHandle
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MethodAttributes Attributes
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
