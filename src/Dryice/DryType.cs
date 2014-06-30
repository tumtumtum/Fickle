using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dryice.Model;

namespace Dryice
{
	public class DryiceType
		: Type
	{
		private readonly ServiceModel serviceModel;
		public ServiceEnum ServiceEnum { get; set; }
		private Type baseType;
		private readonly string name;
		
		public ServiceClass ServiceClass { get; private set; }
		
		public DryiceType(string name)
			: this(name, typeof(object))
		{
		}

		public DryiceType(string name, Type baseType)
		{
			this.name = name;
			this.ServiceClass = null;
			this.baseType = baseType;
		}

		public DryiceType(ServiceClass serviceClass, ServiceModel serviceModel)
		{
			this.serviceModel = serviceModel;
			this.name = serviceClass.Name;
			this.ServiceClass = serviceClass;
		}

		public DryiceType(ServiceEnum serviceEnum, ServiceModel serviceModel)
		{
			this.serviceModel = serviceModel;
			this.ServiceEnum = serviceEnum;
			this.name = serviceEnum.Name;
		}

		protected override bool IsValueTypeImpl()
		{
			return this.IsEnum;
		}

		public override bool IsEnum
		{
			get
			{
				return this.ServiceEnum != null;
			}
		}

		public MethodInfo GetMethod(string name, Type returnType, params Type[] types)
		{
			var i = 0;
			var parameters = types.Select(c => (ParameterInfo)new DryParameterInfo(c, "param" + (i++).ToString(CultureInfo.InvariantCulture)));

			return new DryMethodInfo(this, returnType, name, parameters.ToArray());
		}

		public MethodInfo GetMethod(string name, Type returnType, params ParameterInfo[] parameters)
		{
			return new DryMethodInfo(this, returnType, name, parameters.ToArray());
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

		public override bool ContainsGenericParameters
		{
			get
			{
				return false;
			}
		}

		public override MethodBase DeclaringMethod
		{
			get
			{
				return base.DeclaringMethod;
			}
		}

		public override Type DeclaringType
		{
			get
			{
				return base.DeclaringType;
			}
		}

		public override bool IsSubclassOf(Type c)
		{
			return base.IsSubclassOf(c);
		}

		public override Type ReflectedType
		{
			get
			{
				return base.ReflectedType;
			}
		}

		public override MemberTypes MemberType
		{
			get
			{
				return base.MemberType;
			}
		}

		public override MemberInfo[] GetDefaultMembers()
		{
			return base.GetDefaultMembers();
		}

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			return base.GetMember(name, type, bindingAttr);
		}

		public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		{
			return base.GetMember(name, bindingAttr);
		}

		public override bool IsGenericType
		{
			get
			{
				return base.IsGenericType;
			}
		}

		public override bool IsGenericTypeDefinition
		{
			get
			{
				return base.IsGenericTypeDefinition;
			}
		}

		public override System.Type GetGenericTypeDefinition()
		{
			return base.GetGenericTypeDefinition();
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (returnType == null)
			{
				if (this.ServiceClass != null)
				{
					var returnTypeName = this.ServiceClass.Properties.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).TypeName;

					returnType = this.serviceModel.GetTypeFromName(returnTypeName);
				}
			}

			return new DryPropertyInfo(this, returnType, name);
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

		public virtual ConstructorInfo GetConstructor(string name)
		{
			return new DryConstructorInfo(this, name, new ParameterInfo[0]);
		}

		public virtual ConstructorInfo GetConstructor(string name, Type parameterType)
		{
			return new DryConstructorInfo(this, name, new ParameterInfo[] { new DryParameterInfo(parameterType, name) });
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			var i = 0;

			var parameters = types.Select(c => (ParameterInfo)new DryParameterInfo(c, "param" + (i++).ToString())).ToArray();

			return new DryConstructorInfo(this, "ctor", parameters);
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
				return typeof(DryiceType).Assembly;
			}
		}

		public override string Namespace
		{
			get
			{
				return null;
			}
		}

		public override bool IsInstanceOfType(object o)
		{
			return true;
		}

		public override bool IsAssignableFrom(Type c)
		{
			return true;
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

			var typedObject = o as DryiceType;

			if (typedObject == null)
			{
				return false;
			}

			return this.Name.Equals(typedObject.name, StringComparison.InvariantCultureIgnoreCase);
		}

		public override string AssemblyQualifiedName
		{
			get { throw new NotImplementedException(); }
		}

		public override Type BaseType
		{
			get
			{
				return this.baseType;
			}
		}

		public override string FullName
		{
			get { throw new NotImplementedException(); }
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}
	}
}
