using System;
using System.Collections.Generic;

namespace Ormongo
{
	public static class EmbeddedDocumentExtensions
	{
		public static TEmbedded Build<T, TEmbedded>(this T document, Func<T, List<TEmbedded>> embeddedCollectionCallback, TEmbedded newEmbeddedDocument)
			where T : Document<T>
			where TEmbedded : EmbeddedDocument<TEmbedded, T>
		{
			embeddedCollectionCallback(document).Add(newEmbeddedDocument);
			newEmbeddedDocument.Parent = document;
			return newEmbeddedDocument;
		}
	}
}