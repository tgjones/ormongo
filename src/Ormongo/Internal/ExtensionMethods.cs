using System;
using System.Collections;
using System.Linq;

namespace Ormongo.Internal
{
	public static class ExtensionMethods
	{
		public static IEnumerable Cast(this IEnumerable source, Type resultType)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var method = (typeof(Enumerable).GetMethod("Cast")).MakeGenericMethod(resultType);
			return (IEnumerable) method.Invoke(null, new[] { source });
		}

		public static IList ToList(this IEnumerable source, Type resultType)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var method = (typeof(Enumerable).GetMethod("ToList")).MakeGenericMethod(resultType);
			return (IList)method.Invoke(null, new[] { source });
		}
	}
}