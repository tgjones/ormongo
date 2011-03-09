using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Ormongo.Internal
{
	internal static class ExpressionUtility
	{
		public static string GetPropertyName<T, TProperty>(Expression<Func<T, List<TProperty>>> expression)
		{
			var member = expression.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					expression));

			var propertyInfo = member.Member as PropertyInfo;
			if (propertyInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					expression));

			return propertyInfo.Name;
		}
	}
}