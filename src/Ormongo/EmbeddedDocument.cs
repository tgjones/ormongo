using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Ormongo.Internal.Proxying;

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

		public Type GetUnderlyingType()
		{
			return ProxyManager.GetUnderlyingType(this);
		}
	}
}