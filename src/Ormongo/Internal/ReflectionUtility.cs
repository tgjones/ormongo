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

		public static bool IsListOfRawGeneric(Type itemType, Type typeToCheck)
		{
			if (!IsSubclassOfRawGeneric(typeof(List<>), typeToCheck))
				return false;

			var args = typeToCheck.GetGenericArguments();
			return args.Length == 1  && IsSubclassOfRawGeneric(itemType, args[0]);
		}

		public static Type GetTypeOfGenericList(Type typeToCheck)
		{
			if (!IsSubclassOfRawGeneric(typeof(List<>), typeToCheck))
				throw new Exception("Not a generic list");

			var args = typeToCheck.GetGenericArguments();
			if (args.Length != 1)
				throw new Exception("Not a generic list");

			return args[0];
		}
	}
}