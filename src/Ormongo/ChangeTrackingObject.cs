using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ormongo.Internal;

namespace Ormongo
{
	public abstract class ChangeTrackingObject<T>
		where T : ChangeTrackingObject<T>
	{
// ReSharper disable StaticFieldInGenericType
		private static List<PropertyInfo> _cachedProperties;
// ReSharper restore StaticFieldInGenericType

		private static List<PropertyInfo> CachedProperties
		{
			get
			{
				return _cachedProperties ?? (_cachedProperties = typeof(T).GetProperties()
					.Where(pi => !pi.GetIndexParameters().Any() && pi.CanRead && pi.CanWrite)
					.ToList());
			}
		}

		private readonly Dictionary<string, object> _originalValues;

		protected ChangeTrackingObject()
		{
			_originalValues = new Dictionary<string, object>();
		}

		public void ResetChanges()
		{
			_originalValues.Clear();
			foreach (var propertyInfo in CachedProperties)
				_originalValues[propertyInfo.Name] = propertyInfo.GetValue(this, null);
		}
 
		public bool HasValueChanged<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var original = GetOriginalValue(expression);
			var current = expression.Compile()((T)this);
			return !ReferenceEquals(original, current);
		}

		public TProperty GetOriginalValue<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyName = ExpressionUtility.GetPropertyName(expression);
			return (TProperty) _originalValues[propertyName];
		}
	}
}