using MongoDB.Bson;
using MongoDB.Bson.DefaultSerializer;

namespace Ormongo
{
	public abstract class EmbeddedDocument<TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		public ObjectId ID { get; set; }

		[BsonIgnore]
		public TEmbeddedIn Parent
		{
			get;
			set;
		}
	}
}