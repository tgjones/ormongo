using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Ormongo.Internal.Proxying;

namespace Ormongo.Internal.Serialization
{
	public class RelationalDocumentSerializer : BsonBaseSerializer
	{
		private static RelationalDocumentSerializer _instance;

		public static RelationalDocumentSerializer Instance
		{
			get { return _instance ?? (_instance = new RelationalDocumentSerializer()); }
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			throw new NotImplementedException();
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			return ObjectIdSerializer.Instance.Deserialize(bsonReader, typeof(ObjectId), typeof(ObjectId), options);
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			var idPropertyInfo = value.GetType().GetProperty("ID");
			var id = (ObjectId) idPropertyInfo.GetValue(value, null);
			if (id == ObjectId.Empty)
				throw new Exception("Relational associations must be saved before saving the parent relation");
			ObjectIdSerializer.Instance.Serialize(bsonWriter, id.GetType(), id, options);
		}
	}
}