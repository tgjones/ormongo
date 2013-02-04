using System;
using System.Linq.Expressions;
using System.Reflection;

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
			var member = expression.Body as MemberExpression;
			if (member == null)
			{
				var unary = expression.Body as UnaryExpression;
				if (unary != null && unary.NodeType == ExpressionType.Convert)
				{
					member = unary.Operand as MemberExpression;
					if (member == null)
						throw new ArgumentException(string.Format(
							"Expression '{0}' is not a property expression.",
							expression));
				}
				else
					throw new ArgumentException(string.Format(
						"Expression '{0}' refers to a method, not a property.",
						expression));
			}

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

		public static void IncrementPropertyValue<T, TProperty>(T instance, Expression<Func<T, TProperty>> expression, int value)
		{
			var member = expression.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					expression));
			if (member.Type != typeof(int))
				throw new ArgumentException(string.Format(
					"Expression '{0}' does not refer to an integer.",
					expression));

			var addition = Expression.Add(Expression.Convert(member, typeof(int)), Expression.Constant(value));
			var assignment = Expression.Block(Expression.Assign(member, addition));
			var lambda = Expression.Lambda<Action<T>>(assignment, expression.Parameters[0]);
			lambda.Compile()(instance);
		}
	}
}