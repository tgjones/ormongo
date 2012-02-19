using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ormongo
{
	public class Relation<T> : IQueryable<T>
		where T : Document<T>
	{
		private readonly IQueryable<T> _queryable;
		private readonly Action<T> _setPropertiesCallback;

		public Relation(IQueryable<T> queryable, Action<T> setPropertiesCallback)
		{
			_queryable = queryable;
			_setPropertiesCallback = setPropertiesCallback;
		}

		public T Create(T item)
		{
			_setPropertiesCallback(item);
			return Document<T>.Create(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _queryable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Expression Expression
		{
			get { return _queryable.Expression; }
		}

		public Type ElementType
		{
			get { return _queryable.ElementType; }
		}

		public IQueryProvider Provider
		{
			get { return _queryable.Provider; }
		}
	}
}