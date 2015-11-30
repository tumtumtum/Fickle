using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Fickle.Model;

namespace Fickle
{
	public class FickleType
		: Type
	{
		private Type baseType;
		private readonly bool isPrimitive;
		private readonly bool byRef;
		private readonly string name;
		private readonly ServiceModel serviceModel;
		public static readonly Dictionary<string, FickleType> FickleTypeByName = new Dictionary<string, FickleType>();
		private bool isInterface;
		public override string Name => this.name;
		public override string Namespace => null;
		public override Type BaseType => this.baseType;
		public bool Nullable { get; }
		public ServiceEnum ServiceEnum { get; }
		public ServiceClass ServiceClass { get; }
		public override Type UnderlyingSystemType => this;
		public override bool ContainsGenericParameters => false;
		public override Assembly Assembly => typeof(FickleType).Assembly;

		public static FickleType Define(string name, bool byRef = false, bool isPrimitive = false, bool isInterface = false)
		{
			FickleType retval;

			if (!FickleTypeByName.TryGetValue(name, out retval))
			{
				retval = new FickleType(name, byRef, isPrimitive, isInterface);	
			}

			return retval;
		}
		
		public override Type MakeByRefType()
		{
			if (this.ServiceClass != null)
			{
				return new FickleType(this.ServiceClass, this.serviceModel, true);
			}
			else if (this.ServiceEnum != null)
			{
				return new FickleType(this.ServiceEnum, this.serviceModel, this.Nullable, true);
			}
			else
			{
				return new FickleType(this.name, this.baseType, true);
			}
		}

		public FickleType(string name, bool byRef = false, bool isPrimitive = false, bool isInterface = false)
			: this(name, typeof(object))
		{
			this.byRef = byRef;
			this.isPrimitive = isPrimitive;
			this.isInterface = isInterface;
		}

		public FickleType(string name, Type baseType, bool byRef = false, bool isPrimitive = false)
		{
			this.name = name;
			this.ServiceClass = null;
			this.baseType = baseType;
			this.byRef = byRef;
			this.isPrimitive = isPrimitive;
		}

		public FickleType(ServiceClass serviceClass, ServiceModel serviceModel, bool byRef = false, bool isPrimitive = false)
		{
			this.serviceModel = serviceModel;
			this.name = serviceClass.Name;
			this.ServiceClass = serviceClass;
			this.byRef = byRef;
			this.isPrimitive = isPrimitive;
		}

		public FickleType(ServiceEnum serviceEnum, ServiceModel serviceModel, bool nullable = false, bool byRef = false, bool isPrimitive = false)
		{
			this.serviceModel = serviceModel;
			this.ServiceEnum = serviceEnum;
			this.Nullable = nullable;
			this.name = serviceEnum.Name;
			this.byRef = byRef;
			this.isPrimitive = isPrimitive;
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
			var parameters = types.Select(c => (ParameterInfo)new FickleParameterInfo(c, "param" + (i++).ToString(CultureInfo.InvariantCulture)));

			return new FickleMethodInfo(this, returnType, name, parameters.ToArray());
		}

		public MethodInfo GetMethod(string name, Type returnType, params ParameterInfo[] parameters)
		{
			return new FickleMethodInfo(this, returnType, name, parameters.ToArray());
		}

		protected internal void SetBaseType(Type type)
		{
			baseType = type;
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

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
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

		public override string AssemblyQualifiedName
		{
			get { throw new NotImplementedException(); }
		}
		public override string FullName
		{
			get { throw new NotImplementedException(); }
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		#endregion

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
			if (returnType == null)
			{
				if (this.ServiceClass != null)
				{
					var returnTypeName = this.ServiceClass.Properties.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).TypeName;

					returnType = this.serviceModel.GetTypeFromName(returnTypeName);
				}
			}

			return new FicklePropertyInfo(this, returnType, name);
		}
		
		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return typeof(object).GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			if (this.ServiceEnum != null)
			{
				return typeof(StringComparison).Attributes;
			}
			else if (this.isInterface)
			{
				return typeof(IEnumerable).Attributes;
			}
			else
			{
				return typeof(object).Attributes;
			}
		}

		protected override bool IsArrayImpl()
		{
			return false;
		}

		protected override bool IsByRefImpl()
		{
			return byRef;
		}

		protected override bool IsPointerImpl()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl()
		{
			return isPrimitive;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
		}

		public virtual ConstructorInfo GetConstructor(string name)
		{
			return new FickleConstructorInfo(this, name, new ParameterInfo[0]);
		}

		public virtual ConstructorInfo GetConstructor(string name, Type parameterType)
		{
			return new FickleConstructorInfo(this, name, new ParameterInfo[] { new FickleParameterInfo(parameterType, name) });
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			var i = 0;

			var parameters = types.Select(c => (ParameterInfo)new FickleParameterInfo(c, "param" + (i++).ToString())).ToArray();

			return new FickleConstructorInfo(this, "ctor", parameters);
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

			var typedObject = o as FickleType;

			if (typedObject == null)
			{
				return false;
			}

			return this.Name.Equals(typedObject.name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
