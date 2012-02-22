using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ormongo.Internal.Serialization
{
	public class DocumentSerializer : BsonBaseSerializer
	{
		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			object result;
			if (IsRelationalAssociation(bsonReader, nominalType))
			{
				result = RelationalDocumentSerializer.Instance.Deserialize(bsonReader, nominalType, options);
			}
			else
			{
				result = CustomBsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, options);
				if (result != null)
					((IDocument) result).AfterFind();
			}
			return result;
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			object result;
			if (IsRelationalAssociation(bsonReader, nominalType))
			{
				result = RelationalDocumentSerializer.Instance.Deserialize(bsonReader, nominalType, actualType, options);
			}
			else
			{
				result = CustomBsonClassMapSerializer.Instance.Deserialize(bsonReader, nominalType, actualType, options);
				if (result != null)
					((IDocument) result).AfterFind();
			}
			return result;
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (IsRelationalAssociation(bsonWriter, value))
				RelationalDocumentSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
			else
				CustomBsonClassMapSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
		}

		private static bool IsRelationalAssociation(BsonWriter bsonWriter, object value)
		{
			return value != null && bsonWriter.State == BsonWriterState.Value
				&& ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), value.GetType());
		}

		private static bool IsRelationalAssociation(BsonReader bsonReader, Type nominalType)
		{
			return bsonReader.State == BsonReaderState.Value && bsonReader.CurrentBsonType == BsonType.ObjectId
				&& ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), nominalType);
		}

		public override bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
		{
			return CustomBsonClassMapSerializer.Instance.GetDocumentId(document, out id, out idNominalType, out idGenerator);
		}

		public override void SetDocumentId(object document, object id)
		{
			CustomBsonClassMapSerializer.Instance.SetDocumentId(document, id);
		}
	}
}