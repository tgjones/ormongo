using System;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;

namespace Ormongo.Internal
{
	internal static class ExpressionUtility
	{
		public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			return GetPropertyNameInternal(expression);
		}

		private static string GetPropertyNameInternal(LambdaExpression expression)
		{
			var methodCall = expression.Body as MethodCallExpression;
			if (methodCall != null && typeof(BsonDocument).IsAssignableFrom(methodCall.Object.Type) 
				&& methodCall.Arguments.Count == 1 && methodCall.Arguments[0].NodeType == ExpressionType.Constant
				&& methodCall.Arguments[0].Type == typeof(string))
			{
				return ((MemberExpression) methodCall.Object).Member.Name + "."
					+ (string) ((ConstantExpression) methodCall.Arguments[0]).Value;
			}

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

		public static void GetPropertyNameAndValue<T>(Expression<Func<T, bool>> expression,
			out string propertyName, out object value)
		{
			var binary = expression.Body as BinaryExpression;
			if (binary == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' is not a binary expression.",
					expression));

			propertyName = GetPropertyNameInternal(Expression.Lambda(binary.Left));

			var constant = binary.Right as ConstantExpression;
			if (constant != null)
			{
				value = constant.Value;
				return;
			}

			var member = binary.Right as MemberExpression;
			if (member != null)
			{
				value = Expression
					.Lambda<Func<object>>(Expression.Convert(member, typeof(object)))
					.Compile()();
				return;
			}

			var call = binary.Right as MethodCallExpression;
			if (call == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' is not a constant, member or method call expression.",
					binary.Right));

			value = Expression
				.Lambda<Func<object>>(Expression.Convert(call, typeof(object)))
				.Compile()();
		}
	}
}