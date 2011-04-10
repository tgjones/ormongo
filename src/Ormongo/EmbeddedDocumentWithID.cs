using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Ormongo
{
	public abstract class EmbeddedDocumentWithID<TEmbeddedIn> : EmbeddedDocument<TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		[ScaffoldColumn(false)]
		public ObjectId ID { get; set; }
	}
}