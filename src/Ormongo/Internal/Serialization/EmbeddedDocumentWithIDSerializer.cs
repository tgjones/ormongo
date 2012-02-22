using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Ormongo.Internal.Serialization
{
	public class EmbeddedDocumentWithIDSerializer : EmbeddedDocumentSerializer
	{
		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (value != null)
			{
				var idPropertyInfo = value.GetType().GetProperty("ID");
				var id = (ObjectId) idPropertyInfo.GetValue(value, null);
				if (id == ObjectId.Empty)
					idPropertyInfo.SetValue(value, ObjectId.GenerateNewId(), null);
			}

			base.Serialize(bsonWriter, nominalType, value, options);
		}
	}
}