using System;
using MongoDB.Bson.Serialization;

namespace Ormongo.Internal.Serialization
{
	public class SerializationProvider : IBsonSerializationProvider
	{
		public IBsonSerializer GetSerializer(Type type)
		{
			if (ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), type))
				return new DocumentSerializer();
			if (ReflectionUtility.IsSubclassOfRawGeneric(typeof(EmbeddedDocument<>), type))
				return new EmbeddedDocumentSerializer();
			if (type == typeof(Attachment))
				return new AttachmentSerializer();
			return null;
		}
	}
}