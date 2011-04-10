using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Ormongo
{
	public abstract class EmbeddedDocument<TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		[BsonIgnore, ScaffoldColumn(false)]
		public TEmbeddedIn Parent
		{
			get;
			set;
		}
	}
}