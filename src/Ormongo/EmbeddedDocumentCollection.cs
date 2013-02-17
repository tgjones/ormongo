using System.Collections.Generic;

namespace Ormongo
{
	public class EmbeddedDocumentCollection<T, TEmbeddedIn> : List<T>
		where T : EmbeddedDocument<T, TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		private readonly TEmbeddedIn _embeddedIn;

		public EmbeddedDocumentCollection(TEmbeddedIn embeddedIn)
		{
			_embeddedIn = embeddedIn;
		}

		public void Create(T item)
		{
			Add(item);
			_embeddedIn.Save();
		}
	}
}