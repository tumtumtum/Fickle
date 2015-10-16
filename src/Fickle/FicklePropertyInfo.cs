using System;
using System.Globalization;
using System.Reflection;

namespace Fickle
{
	public class FicklePropertyInfo
		: PropertyInfo
	{
		private readonly string name; 
		private readonly Type declaringType;
		private readonly Type propertyType;
		
		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override string Name => this.name;
		public override Type PropertyType => this.propertyType;
		public override Type DeclaringType => this.declaringType;
		public override Type ReflectedType => this.declaringType;

		public FicklePropertyInfo(Type declaringType, Type propertyType, string name)
		{
			this.name = name;
			this.propertyType = propertyType;
			this.declaringType = declaringType;
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return new FickleMethodInfo(this.declaringType, this.propertyType, "get_" + this.name, new ParameterInfo[0]);
		}

		#region Unimplemented

		public override PropertyAttributes Attributes
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

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			throw new NotImplementedException();
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
