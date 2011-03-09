using MongoDB.Bson;

namespace Ormongo
{
	// TODO: Should know how to access its parent document.
	public class EmbeddedDocument
	{
		public ObjectId ID { get; set; }
	}
}