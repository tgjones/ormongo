using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.DefaultSerializer;

namespace Ormongo
{
	public abstract class EmbeddedDocument<TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		[ScaffoldColumn(false)]
		public ObjectId ID { get; set; }

		[BsonIgnore, ScaffoldColumn(false)]
		public TEmbeddedIn Parent
		{
			get;
			set;
		}
	}
}