using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Bson.DefaultSerializer;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Ormongo.Internal;

namespace Ormongo
{
	public class Document<T>
	{
		[BsonId]
		public ObjectId ID { get; set; }

		private static MongoCollection<T> GetCollection()
		{
			string collectionName = typeof (T).Name;
			return OrmongoConfiguration.GetMongoDatabase().GetCollection<T>(collectionName);
		}

		private static IMongoQuery GetIDQuery(ObjectId id)
		{
			return Query.EQ("_id", id);
		}

		public static void Delete(ObjectId id)
		{
			GetCollection().Remove(GetIDQuery(id));
		}

		public static void DeleteAll()
		{
			GetCollection().RemoveAll();
		}

		public static T FindByID(ObjectId id)
		{
			return GetCollection().FindOne(GetIDQuery(id));
		}

		public static IQueryable<T> Find(Expression<Func<T, bool>> predicate)
		{
			return FindAll().Where(predicate);
		}

		public static IQueryable<T> FindAll()
		{
			return GetCollection().AsQueryable();
		}

		public static void Push<TProperty>(ObjectId id, Expression<Func<T, List<TProperty>>> expression, TProperty value)
		{
			UpdateBuilder update = Update.Push(ExpressionUtility.GetPropertyName(expression), BsonDocumentWrapper.Create(value));
			GetCollection().Update(GetIDQuery(id), update);
		}

		public void Save()
		{
			GetCollection().Save(this);
		}
	}
}