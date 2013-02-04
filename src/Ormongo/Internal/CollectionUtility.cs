using System;
using System.Linq;

namespace Ormongo.Internal
{
	public static class CollectionUtility
	{
		public static string GetCollectionName(Type type)
		{
			while (type != null && type != typeof(object))
			{
				var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
				if (typeof(Document<>) == cur)
					return type.GetGenericArguments().First().Name;
				type = type.BaseType;
			}
			throw new Exception("Type doesn't inherit from Document<>");
		}
	}
}