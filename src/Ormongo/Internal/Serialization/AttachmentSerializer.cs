using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ormongo.Internal.Serialization
{
	public class AttachmentSerializer : BsonBaseSerializer
	{
		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			if (bsonReader.CurrentBsonType == BsonType.Null)
			{
				bsonReader.ReadNull();
				return null;
			}

			var id = (ObjectId)ObjectIdSerializer.Instance.Deserialize(bsonReader, nominalType, options);
			return new Attachment(id);
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (value == null)
			{
				bsonWriter.WriteNull();
			}
			else
			{
				var attachment = (Attachment)value;
				ObjectIdSerializer.Instance.Serialize(bsonWriter, nominalType, attachment.ID, options);
			}
		}

		public override bool GetDocumentId(object document, out object id, out IIdGenerator idGenerator)
		{
			throw new NotSupportedException();
		}

		public override void SetDocumentId(object document, object id)
		{
			throw new NotSupportedException();
		}
	}
}