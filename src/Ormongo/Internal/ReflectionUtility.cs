using System;

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
	}
}