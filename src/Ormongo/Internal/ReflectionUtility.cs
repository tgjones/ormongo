using System;
using System.Collections.Generic;

namespace Ormongo.Internal
{
	internal static class ReflectionUtility
	{
		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
					return true;
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static Type GetTypeOfRawGeneric(Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
					return toCheck.GetGenericArguments()[0];
				toCheck = toCheck.BaseType;
			}
			return null;
		}

		public static bool IsListOfRawGeneric(Type itemType, Type typeToCheck)
		{
			if (!IsSubclassOfRawGeneric(typeof(List<>), typeToCheck))
				return false;

			while (typeToCheck != null && typeToCheck != typeof (object))
			{
				var args = typeToCheck.GetGenericArguments();
				if (args.Length == 1 && IsSubclassOfRawGeneric(itemType, args[0]))
					return true;
				typeToCheck = typeToCheck.BaseType;
			}
			return false;
		}

		public static bool IsListOfType(Type itemType, Type typeToCheck)
		{
			if (!IsSubclassOfRawGeneric(typeof(List<>), typeToCheck))
				return false;

			var args = typeToCheck.GetGenericArguments();
			return args.Length == 1 && itemType == args[0];
		}

		public static Type GetTypeOfGenericList(Type typeToCheck)
		{
			if (!IsSubclassOfRawGeneric(typeof(List<>), typeToCheck))
				throw new Exception("Not a generic list");

			while (typeToCheck != null && typeToCheck != typeof(object))
			{
				var args = typeToCheck.GetGenericArguments();
				if (args.Length == 1)
					return args[0];
				typeToCheck = typeToCheck.BaseType;
			}

			throw new Exception("Not a generic list");
		}
	}
}