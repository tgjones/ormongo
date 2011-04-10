using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ormongo.Internal.Serialization
{
	public class DocumentSerializer : BsonBaseSerializer
	{
		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			object result = BsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, options);
			AssociationUtility.UpdateAssociations(result);
			return result;
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			object result = BsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, actualType, options);
			AssociationUtility.UpdateAssociations(result);
			return result;
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			BsonClassMapSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
		}

		public override bool GetDocumentId(object document, out object id, out IIdGenerator idGenerator)
		{
			return BsonClassMapSerializer.Instance.GetDocumentId(document, out id, out idGenerator);
		}

		public override void SetDocumentId(object document, object id)
		{
			BsonClassMapSerializer.Instance.SetDocumentId(document, id);
		}
	}
}