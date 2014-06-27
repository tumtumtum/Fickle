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
			AddPrimitiveType(typeof(byte));
			AddPrimitiveType(typeof(byte?));
			AddPrimitiveType(typeof(char));
			AddPrimitiveType(typeof(char?));
			AddPrimitiveType(typeof(short));
			AddPrimitiveType(typeof(short?));
			AddPrimitiveType(typeof(int));
			AddPrimitiveType(typeof(int?));
			AddPrimitiveType(typeof(int), "int");
			AddPrimitiveType(typeof(int?), "int?"); 
			AddPrimitiveType(typeof(long));
			AddPrimitiveType(typeof(long?));
			AddPrimitiveType(typeof(long), "long");
			AddPrimitiveType(typeof(long?), "long?");
			AddPrimitiveType(typeof(double));
			AddPrimitiveType(typeof(double?));
			AddPrimitiveType(typeof(string));
			AddPrimitiveType(typeof(DateTime));
			AddPrimitiveType(typeof(DateTime?));
			AddPrimitiveType(typeof(TimeSpan));
			AddPrimitiveType(typeof(TimeSpan?));
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
