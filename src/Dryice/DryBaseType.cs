// Copyright (c) 2013 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Globalization;
using System.Reflection;

namespace Fickle
{
	public class DryBaseType
		: Type
	{
		private readonly string name;

		public DryBaseType(string name)
		{
			this.name = name;
		}

		#region Unimplemented

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

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			return new MethodInfo[0];
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

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotImplementedException();
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

		#endregion

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return typeof(object).GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public override Type GetElementType()
		{
			return null;
		}

		protected override bool HasElementTypeImpl()
		{
			return false;
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

		public override Type UnderlyingSystemType
		{
			get
			{
				return this;
			}
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return null;
		}

		public override string Name
		{
			get
			{
				return this.name;
			}
		}

		public override Assembly Assembly
		{
			get
			{
				return null;
			}
		}

		public override Type BaseType
		{
			get
			{
				return typeof(object);
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return new object[0];
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

			var typedObject = o as DryBaseType;

			if (typedObject == null)
			{
				return false;
			}

			return this.Name.Equals(typedObject.name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
