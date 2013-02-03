using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Ormongo
{
	public abstract class EmbeddedDocumentWithID<T, TEmbeddedIn> : EmbeddedDocument<T, TEmbeddedIn>
		where T : EmbeddedDocument<T, TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn> 
	{
		[ScaffoldColumn(false)]
		public ObjectId ID { get; set; }
	}
}