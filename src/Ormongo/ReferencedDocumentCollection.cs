using System;
using System.Collections.Generic;

namespace Ormongo
{
	public class ReferencedDocumentCollection<T1, T2> : List<T1>
		where T1 : Document<T1>
		where T2 : Document<T2>
	{
		private readonly T2 _referencedIn;
		private readonly Action<T1> _setReferencingItemCallback;

		public ReferencedDocumentCollection(T2 referencedIn, Action<T1> setReferencingItemCallback)
		{
			_referencedIn = referencedIn;
			_setReferencingItemCallback = setReferencingItemCallback;
		}

		public T1 Create(T1 item)
		{
			Add(item);
			_setReferencingItemCallback(item);
			item.Save();
			_referencedIn.Save();
			return item;
		}
	}
}