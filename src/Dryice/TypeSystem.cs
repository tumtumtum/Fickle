using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dryice
{
	public class TypeSystem
	{
		private static readonly HashSet<Type> primitiveTypes = new HashSet<Type>();
		private static readonly Dictionary<string, Type> primitiveTypeByName = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		static void AddPrimitiveType(Type type, string name = null)
		{
			primitiveTypes.Add(type);

			if (name == null)
			{
				Type underlyingType;

				if ((underlyingType = Nullable.GetUnderlyingType(type)) == null)
				{
					name = type.Name;
				}
				else
				{
					name = underlyingType.Name + "?";
				}
			}

			primitiveTypeByName[name] = type;
		}

		static TypeSystem()
		{
			AddPrimitiveType(typeof(byte), "byte");
			AddPrimitiveType(typeof(byte?), "byte?");
			AddPrimitiveType(typeof(char), "char");
			AddPrimitiveType(typeof(char?), "char?");
			AddPrimitiveType(typeof(short), "short");
			AddPrimitiveType(typeof(short?), "short?");
			AddPrimitiveType(typeof(int), "int");
			AddPrimitiveType(typeof(int?), "int?"); 
			AddPrimitiveType(typeof(long), "long");
			AddPrimitiveType(typeof(long?), "long?");
			AddPrimitiveType(typeof(double), "double");
			AddPrimitiveType(typeof(double?), "double?");
			AddPrimitiveType(typeof(string), "string");
			AddPrimitiveType(typeof(DateTime), "datetime");
			AddPrimitiveType(typeof(DateTime?), "datetime?");
			AddPrimitiveType(typeof(TimeSpan), "timespan");
			AddPrimitiveType(typeof(TimeSpan?), "timespan?");
			AddPrimitiveType(typeof(Guid), "uuid");
			AddPrimitiveType(typeof(Guid?), "uuid?");
		}

		public static bool IsPrimitiveType(Type type)
		{
			return primitiveTypes.Contains(type);
		}

		public static bool IsNotPrimitiveType(Type type)
		{
			return !primitiveTypes.Contains(type);
		}

		public static Type GetPrimitiveType(string name)
		{
			Type type;

			primitiveTypeByName.TryGetValue(name, out type);

			return type;
		}
	}
}
