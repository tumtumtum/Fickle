using System;
using System.Collections.Generic;
using System.Linq;

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
			AddPrimitiveType(typeof(bool), "bool");
			AddPrimitiveType(typeof(bool?), "bool?");
			AddPrimitiveType(typeof(byte), "Byte");
			AddPrimitiveType(typeof(byte?), "Byte?");
			AddPrimitiveType(typeof(char), "Char");
			AddPrimitiveType(typeof(char?), "Char?");
			AddPrimitiveType(typeof(short), "Short");
			AddPrimitiveType(typeof(short?), "Short?");
			AddPrimitiveType(typeof(int), "Int");
			AddPrimitiveType(typeof(int?), "Int?"); 
			AddPrimitiveType(typeof(long), "Long");
			AddPrimitiveType(typeof(long?), "Long?");
			AddPrimitiveType(typeof(double), "Double");
			AddPrimitiveType(typeof(double?), "Double?");
			AddPrimitiveType(typeof(string), "String");
			AddPrimitiveType(typeof(DateTime), "DateTime");
			AddPrimitiveType(typeof(DateTime?), "DateTime?");
			AddPrimitiveType(typeof(TimeSpan), "TimeSpan");
			AddPrimitiveType(typeof(TimeSpan?), "TimeSpan?");
			AddPrimitiveType(typeof(Guid), "UUID");
			AddPrimitiveType(typeof(Guid?), "UUID?");
		}

		public static string GetPrimitiveName(Type type)
		{
			return primitiveTypeByName.FirstOrDefault(c => c.Value == type).Key;
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
