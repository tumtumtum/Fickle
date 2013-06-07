using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Sublimate.Model;

namespace Sublimate
{
	public class SublimateType
		: Type
	{
		private Type baseType;
		private readonly string name;
		
		public ServiceType ServiceType { get; private set; }

		public SublimateType(string name)
			: this(name, typeof(object))
		{
		}

		public SublimateType(string name, Type baseType)
		{
			this.name = name;
			this.ServiceType = null;
			this.baseType = baseType;
		}

		public SublimateType(ServiceType serviceType)
		{
			this.name = serviceType.Name;
			this.ServiceType = serviceType;
		}

		public MethodInfo GetMethod(string name, Type returnType, params Type[] types)
		{
			var i = 0;
			var parameters = types.Select(c => (ParameterInfo)new SublimateParameterInfo(c, "param" + (i++).ToString()));

			return new SublimateMethodInfo(this, returnType, name, parameters.ToArray());
		}

		protected internal void SetBaseType(Type type)
		{
			baseType = type;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetInterfaces()
		{
			throw new NotImplementedException();
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetElementType()
		{
			return this;
		}

		protected override bool HasElementTypeImpl()
		{
			return false;
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return new SublimatePropertyInfo(this, returnType, name);
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return typeof(object).GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			return typeof(object).Attributes;
		}

		protected override bool IsArrayImpl()
		{
			return false;
		}

		protected override bool IsByRefImpl()
		{
			return false;
		}

		protected override bool IsPointerImpl()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotImplementedException();
		}

		public override Type UnderlyingSystemType
		{
			get
			{
				return this;
			}
		}

		public virtual ConstructorInfo GetConstructor(string name, Type parameterType)
		{
			return new SublimateConstructorInfo(this, name, new ParameterInfo[] { new SublimateParameterInfo(parameterType, name) });
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			var i = 0;

			var parameters = types.Select(c => (ParameterInfo)new SublimateParameterInfo(c, "param" + (i++).ToString())).ToArray();

			return new SublimateConstructorInfo(this, "ctor", parameters);
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override Guid GUID
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Module Module
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Assembly Assembly
		{
			get
			{
				return typeof(SublimateType).Assembly;
			}
		}

		public override string FullName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string Namespace
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string AssemblyQualifiedName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Type BaseType
		{
			get
			{
				return baseType;
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o == (object)this)
			{
				return true;
			}
			var typedObject = o as SublimateType;

			if (typedObject == null)
			{
				return false;
			}

			return this.Name.Equals(typedObject.name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
