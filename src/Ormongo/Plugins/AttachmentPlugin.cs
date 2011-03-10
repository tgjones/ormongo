using System;
using MongoDB.Bson;
using MongoDB.Bson.DefaultSerializer;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Ormongo.Plugins
{
	public class AttachmentPlugin : IPlugin
	{
		public void Initialize()
		{
			BsonSerializer.RegisterSerializer(typeof (Attachment), new AttachmentSerializer());
		}

		private class AttachmentSerializer : BsonBaseSerializer
		{
			public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
			{
				if (bsonReader.CurrentBsonType == BsonType.Null)
				{
					bsonReader.ReadNull();
					return null;
				}

				var id = (ObjectId) ObjectIdSerializer.Instance.Deserialize(bsonReader, nominalType, options);
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
					var attachment = (Attachment) value;
					ObjectIdSerializer.Instance.Serialize(bsonWriter, nominalType, attachment.ID, options);					
				}
			}
		}
	}
}