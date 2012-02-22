using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ormongo.Internal.Serialization
{
	public class RelationCollectionSerializer : BsonBaseSerializer
	{
		private static RelationCollectionSerializer _instance;

		public static RelationCollectionSerializer Instance
		{
			get { return _instance ?? (_instance = new RelationCollectionSerializer()); }
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			throw new NotImplementedException();
		}

		public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			return EnumerableSerializer.Instance.Deserialize(bsonReader, typeof(ArrayList), typeof(ArrayList), options);
		}

		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (value == null)
			{
				bsonWriter.WriteNull();
				return;
			}

			var result = new List<ObjectId>();
			foreach (var item in (IEnumerable)value)
			{
				var idPropertyInfo = item.GetType().GetProperty("ID");
				var id = (ObjectId) idPropertyInfo.GetValue(item, null);
				if (id == ObjectId.Empty)
					throw new Exception("Relational associations must be saved before saving the parent relation");
				result.Add(id);
			}
			EnumerableSerializer.Instance.Serialize(bsonWriter, result.GetType(), result, options);
		}
	}
}