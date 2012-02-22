using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using Ormongo.Internal.Proxying;

namespace Ormongo.Internal.Serialization
{
	/// <summary>
	/// Copied from MongoDB-CSharp BsonClassMapSerializer
	/// </summary>
	public class CustomBsonClassMapSerializer : IBsonSerializer
	{
		private static CustomBsonClassMapSerializer instance = new CustomBsonClassMapSerializer();

		public static CustomBsonClassMapSerializer Instance
		{
			get
			{
				return CustomBsonClassMapSerializer.instance;
			}
		}

		static CustomBsonClassMapSerializer()
		{
		}

		public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			this.VerifyNominalType(nominalType);
			if (bsonReader.CurrentBsonType == BsonType.Null)
			{
				bsonReader.ReadNull();
				return (object)null;
			}
			else
			{
				Type actualType = BsonDefaultSerializer.LookupDiscriminatorConvention(nominalType).GetActualType(bsonReader, nominalType);
				if (actualType != nominalType)
				{
					IBsonSerializer bsonSerializer = BsonSerializer.LookupSerializer(actualType);
					if (bsonSerializer != this)
						return bsonSerializer.Deserialize(bsonReader, nominalType, actualType, options);
				}
				return this.Deserialize(bsonReader, nominalType, actualType, options);
			}
		}

		public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			this.VerifyNominalType(nominalType);
			if (bsonReader.CurrentBsonType == BsonType.Null)
			{
				bsonReader.ReadNull();
				return (object)null;
			}
			else
			{
				if (actualType.IsValueType)
					throw new BsonSerializationException(string.Format("Value class {0} cannot be deserialized.", (object)actualType.FullName));
				BsonClassMap bsonClassMap = BsonClassMap.LookupClassMap(actualType);
				if (bsonClassMap.IsAnonymous)
					throw new InvalidOperationException("An anonymous class cannot be deserialized.");
				// Added
				object instance = ProxyManager.GetProxy(bsonClassMap.ClassType);
				// Added
				if (bsonReader.CurrentBsonType != BsonType.Document)
					throw new FileFormatException(string.Format("Expected a nested document representing the serialized form of a {0} value, but found a value of type {1} instead.", (object)actualType.FullName, (object)bsonReader.CurrentBsonType));
				bsonReader.ReadStartDocument();
				HashSet<BsonMemberMap> hashSet = new HashSet<BsonMemberMap>(bsonClassMap.MemberMaps);
				IDiscriminatorConvention discriminatorConvention = BsonDefaultSerializer.LookupDiscriminatorConvention(nominalType);
				while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
				{
					string elementName = bsonReader.ReadName();
					if (elementName == discriminatorConvention.ElementName)
					{
						bsonReader.SkipValue();
					}
					else
					{
						BsonMemberMap memberMapForElement = bsonClassMap.GetMemberMapForElement(elementName);
						if (memberMapForElement != null && memberMapForElement != bsonClassMap.ExtraElementsMemberMap)
						{
							this.DeserializeMember(bsonReader, instance, memberMapForElement);
							hashSet.Remove(memberMapForElement);
						}
						else if (bsonClassMap.ExtraElementsMemberMap != null)
						{
							this.DeserializeExtraElement(bsonReader, instance, elementName, bsonClassMap.ExtraElementsMemberMap);
						}
						else
						{
							if (!bsonClassMap.IgnoreExtraElements)
								throw new FileFormatException(string.Format("Element '{0}' does not match any field or property of class {1}.", (object)elementName, (object)bsonClassMap.ClassType.FullName));
							bsonReader.SkipValue();
						}
					}
				}
				bsonReader.ReadEndDocument();
				foreach (BsonMemberMap bsonMemberMap in hashSet)
				{
					if (bsonMemberMap.IsRequired)
					{
						string str = bsonMemberMap.MemberInfo.MemberType == MemberTypes.Field ? "field" : "property";
						throw new FileFormatException(string.Format("Required element '{0}' for {1} '{2}' of class {3} is missing.", (object)bsonMemberMap.ElementName, (object)str, (object)bsonMemberMap.MemberName, (object)bsonClassMap.ClassType.FullName));
					}
					else if (bsonMemberMap.HasDefaultValue)
						bsonMemberMap.ApplyDefaultValue(instance);
				}
				return instance;
			}
		}

		public bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
		{
			BsonMemberMap idMemberMap = BsonClassMap.LookupClassMap(document.GetType()).IdMemberMap;
			if (idMemberMap != null)
			{
				id = idMemberMap.Getter(document);
				idNominalType = idMemberMap.MemberType;
				idGenerator = idMemberMap.IdGenerator;
				return true;
			}
			else
			{
				id = (object)null;
				idNominalType = (Type)null;
				idGenerator = (IIdGenerator)null;
				return false;
			}
		}

		public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (value == null)
			{
				bsonWriter.WriteNull();
			}
			else
			{
				if (nominalType.IsGenericType && nominalType.GetGenericTypeDefinition() == typeof(Nullable<>))
					nominalType = nominalType.GetGenericArguments()[0];
				this.VerifyNominalType(nominalType);
				Type type = value == null ? nominalType : value.GetType();
				BsonClassMap bsonClassMap = BsonClassMap.LookupClassMap(type);
				bsonWriter.WriteStartDocument();
				DocumentSerializationOptions serializationOptions = options == null ? DocumentSerializationOptions.Defaults : (DocumentSerializationOptions)options;
				BsonMemberMap memberMap = (BsonMemberMap)null;
				if (serializationOptions.SerializeIdFirst)
				{
					memberMap = bsonClassMap.IdMemberMap;
					if (memberMap != null)
						this.SerializeMember(bsonWriter, value, memberMap);
				}
				if ((type != nominalType || bsonClassMap.DiscriminatorIsRequired || bsonClassMap.HasRootClass) && !bsonClassMap.IsAnonymous)
				{
					IDiscriminatorConvention discriminatorConvention = BsonDefaultSerializer.LookupDiscriminatorConvention(nominalType);
					BsonValue discriminator = discriminatorConvention.GetDiscriminator(nominalType, type);
					if (discriminator != (BsonValue)null)
					{
						bsonWriter.WriteName(discriminatorConvention.ElementName);
						discriminator.WriteTo(bsonWriter);
					}
				}
				foreach (BsonMemberMap bsonMemberMap in bsonClassMap.MemberMaps)
				{
					if (bsonMemberMap != memberMap)
					{
						if (bsonMemberMap == bsonClassMap.ExtraElementsMemberMap)
							this.SerializeExtraElements(bsonWriter, value, bsonMemberMap);
						else
							this.SerializeMember(bsonWriter, value, bsonMemberMap);
					}
				}
				bsonWriter.WriteEndDocument();
			}
		}

		public void SetDocumentId(object document, object id)
		{
			Type type = document.GetType();
			if (type.IsValueType)
				throw new BsonSerializationException(string.Format("SetDocumentId cannot be used with value type {0}.", (object)type.FullName));
			BsonMemberMap idMemberMap = BsonClassMap.LookupClassMap(type).IdMemberMap;
			if (idMemberMap == null)
				throw new InvalidOperationException(string.Format("Class {0} has no Id member.", (object)document.GetType().FullName));
			idMemberMap.Setter(document, id);
		}

		private void DeserializeExtraElement(BsonReader bsonReader, object obj, string elementName, BsonMemberMap extraElementsMemberMap)
		{
			BsonDocument bsonDocument = (BsonDocument)extraElementsMemberMap.Getter(obj);
			if (bsonDocument == (BsonDocument)null)
			{
				bsonDocument = new BsonDocument();
				extraElementsMemberMap.Setter(obj, (object)bsonDocument);
			}
			BsonValue bsonValue = BsonValue.ReadFrom(bsonReader);
			bsonDocument[elementName] = bsonValue;
		}

		private void DeserializeMember(BsonReader bsonReader, object obj, BsonMemberMap memberMap)
		{
			try
			{
				Type memberType = memberMap.MemberType;
				Type actualType = bsonReader.CurrentBsonType != BsonType.Null ? BsonDefaultSerializer.LookupDiscriminatorConvention(memberType).GetActualType(bsonReader, memberType) : memberType;
				object obj1 = memberMap.GetSerializer(actualType).Deserialize(bsonReader, memberType, actualType, memberMap.SerializationOptions);
				// --- Added
				if (IsRelation(memberMap))
				{
					ValidateVirtualRelation(memberMap);
					((IDocument) obj).RelationalIDs[memberMap.MemberName] = (ObjectId) obj1;
				}
				// --- Added
				else
					memberMap.Setter(obj, obj1);
			}
			catch (Exception ex)
			{
				throw new FileFormatException(string.Format("An error occurred while deserializing the {0} {1} of class {2}: {3}", (object)memberMap.MemberName, memberMap.MemberInfo.MemberType == MemberTypes.Field ? (object)"field" : (object)"property", (object)obj.GetType().FullName, (object)ex.Message), ex);
			}
		}

		private void SerializeExtraElements(BsonWriter bsonWriter, object obj, BsonMemberMap extraElementsMemberMap)
		{
			BsonDocument bsonDocument = (BsonDocument)extraElementsMemberMap.Getter(obj);
			if (!(bsonDocument != (BsonDocument)null))
				return;
			foreach (BsonElement bsonElement in bsonDocument)
			{
				bsonWriter.WriteName(bsonElement.Name);
				bsonElement.Value.WriteTo(bsonWriter);
			}
		}

		private void SerializeMember(BsonWriter bsonWriter, object obj, BsonMemberMap memberMap)
		{
			object objA = memberMap.Getter(obj);
			if (objA == null && memberMap.IgnoreIfNull || (memberMap.HasDefaultValue && !memberMap.SerializeDefaultValue && object.Equals(objA, memberMap.DefaultValue) || !memberMap.ShouldSerializeMethod(obj)))
				return;
			bsonWriter.WriteName(memberMap.ElementName);
			Type memberType = memberMap.MemberType;
			Type actualType = objA == null ? memberType : objA.GetType();
			if (IsRelation(memberMap))
				ValidateVirtualRelation(memberMap);
			memberMap.GetSerializer(actualType).Serialize(bsonWriter, memberType, objA, memberMap.SerializationOptions);
		}

		// Added
		private static bool IsRelation(BsonMemberMap memberMap)
		{
			return ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), memberMap.MemberType);
		}

		private static void ValidateVirtualRelation(BsonMemberMap memberMap)
		{
			if (!((PropertyInfo)memberMap.MemberInfo).GetGetMethod().IsVirtual)
				throw new Exception("Relational association properties must be declared as virtual, to support lazy loading");
		}
		// Added

		private void VerifyNominalType(Type nominalType)
		{
			if (!nominalType.IsClass && (!nominalType.IsValueType || nominalType.IsPrimitive) && !nominalType.IsInterface || typeof(Array).IsAssignableFrom(nominalType))
				throw new BsonSerializationException(string.Format("BsonClassMapSerializer cannot be used with type {0}.", (object)nominalType.FullName));
		}
	}
}