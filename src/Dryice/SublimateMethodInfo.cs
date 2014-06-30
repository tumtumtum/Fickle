using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dryice
{
	public class DryiceMethodInfo
		: MethodInfo
	{
		private readonly string name;
		private readonly Type returnType;
		private readonly Type declaringType;
		private readonly MethodAttributes methodAttributes;
		private readonly ParameterInfo[] parameters;

		public DryiceMethodInfo(Type declaringType, Type returnType, string name, ParameterInfo[] parameters, bool isStatic = false)
		{
			this.name = name;
			this.returnType = returnType;
			this.declaringType = declaringType;
			this.parameters = parameters;
			this.methodAttributes = MethodAttributes.Public;

			if (isStatic)
			{
				this.methodAttributes |= MethodAttributes.Static;
			}
		}

		public override Type ReturnType
		{
			get
			{
				return returnType;
			}
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

		public override MethodInfo GetBaseDefinition()
		{
			throw new NotImplementedException();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string Name
		{
			get
			{
				return name;
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
				return methodAttributes;
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
