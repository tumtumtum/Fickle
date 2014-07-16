using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dryice.Model;

namespace Dryice
{
	public class DryType
		: Type
	{
		private Type baseType;
		private readonly bool byRef;
		private readonly string name;
		private readonly ServiceModel serviceModel;
		public ServiceEnum ServiceEnum { get; private set; }
		public bool Nullable { get; private set; }
		public ServiceClass ServiceClass { get; private set; }
		private static readonly Dictionary<string, DryType> dryTypeByName = new Dictionary<string, DryType>();

		public override string Name { get { return name; } }
		public override string Namespace { get { return null; } }
		public override Type BaseType { get { return this.baseType; } }
		public override Type UnderlyingSystemType { get { return this; } }
		public override bool ContainsGenericParameters { get { return false; } }
		public override Assembly Assembly { get { return typeof(DryType).Assembly; } }
		
		public static DryType Define(string name, bool byRef = false)
		{
			DryType retval;

			if (!dryTypeByName.TryGetValue(name, out retval))
			{
				retval = new DryType(name, byRef);	
			}

			return retval;
		}
		
		public DryType(string name, bool byRef = false)
			: this(name, typeof(object))
		{
			this.byRef = byRef;
		}

		public static DryType Make(string name)
		{
			return new DryType(name);
		}

		public static DryType Make(string name, Type baseType)
		{
			return new DryType(name, baseType);
		}

		public DryType(string name, Type baseType)
		{
			this.name = name;
			this.ServiceClass = null;
			this.baseType = baseType;
		}

		public DryType(ServiceClass serviceClass, ServiceModel serviceModel)
		{
			this.serviceModel = serviceModel;
			this.name = serviceClass.Name;
			this.ServiceClass = serviceClass;
		}

		public DryType(ServiceEnum serviceEnum, ServiceModel serviceModel, bool nullable = false)
		{
			this.serviceModel = serviceModel;
			this.ServiceEnum = serviceEnum;
			this.Nullable = nullable;
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

			return new DryPropertyInfo(this, returnType, name);
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
			return false;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
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

			var typedObject = o as DryType;

			if (typedObject == null)
			{
				return false;
			}

			return this.Name.Equals(typedObject.name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
