using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sublimate
{
	public class TypeSystem
	{
		private static readonly HashSet<Type> primitiveTypes = new HashSet<Type>();
		private static readonly Dictionary<string, Type> primitiveTypeByName = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		static void AddPrimitiveType(Type type, string name)
		{
			primitiveTypes.Add(type); 
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
			AddPrimitiveType(typeof(string), "String");
			AddPrimitiveType(typeof(DateTime), "DateTime");
			AddPrimitiveType(typeof(DateTime?), "DateTime?");
			AddPrimitiveType(typeof(TimeSpan), "TimeSpan");
			AddPrimitiveType(typeof(TimeSpan?), "TimeSpan?");
			AddPrimitiveType(typeof(Guid), "uuid");
			AddPrimitiveType(typeof(Guid?), "uuid?");
		}

		public static bool IsPrimitiveType(Type type)
		{
			return primitiveTypes.Contains(type);
		}

		public static Type GetPrimitiveType(string name)
		{
			Type type;

			primitiveTypeByName.TryGetValue(name, out type);

			return type;
		}
	}
}
