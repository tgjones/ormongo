using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Ormongo.Plugins;

namespace Ormongo.Internal.Serialization
{
	public class DocumentSerializer : BsonBaseSerializer
	{
		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			object result = BsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, options);
			PluginManager.Execute(p => p.AfterFind(result));
			return result;
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			object result = BsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, actualType, options);
			PluginManager.Execute(p => p.AfterFind(result));
			return result;
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			BsonClassMapSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
		}

		public override bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
		{
			return BsonClassMapSerializer.Instance.GetDocumentId(document, out id, out idNominalType, out idGenerator);
		}

		public override void SetDocumentId(object document, object id)
		{
			BsonClassMapSerializer.Instance.SetDocumentId(document, id);
		}
	}
}