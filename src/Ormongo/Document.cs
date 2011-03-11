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
using Ormongo.Plugins;

namespace Ormongo
{
	public class Document<T>
		where T : Document<T>
	{
		public static EventHandler<DocumentSavingEventArgs<T>> Saving;

		static Document()
		{
			// Initialize plugins.
			PluginManager.Execute(p => p.Initialize());

			// Start by auto-mapping members.
			Type type = typeof (T);
			var classMap = new DocumentClassMap(type);
			classMap.AutoMap();

			// Tell Mongo about this.
			BsonClassMap.RegisterClassMap(classMap);
		}

		[BsonId]
		public ObjectId ID { get; set; }

		internal static MongoCollection<T> GetCollection()
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

		public static void Drop()
		{
			try
			{
				GetCollection().Drop();
			}
			catch (MongoCommandException ex)
			{
				if (!ex.CommandResult.ErrorMessage.Contains("ns not found"))
					throw;
			}
		}

		public static T FindByID(ObjectId id)
		{
			return GetCollection().FindOneById(id);
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
			if (Saving != null)
				Saving(this, new DocumentSavingEventArgs<T>((T) this));

			PluginManager.Execute(p => p.BeforeSave(this));

			GetCollection().Save(this);
		}

		#region Indexing methods

		public static void EnsureIndex<TProperty>(params Expression<Func<T, TProperty>>[] expression)
		{
			IMongoIndexKeys indexKeys = IndexKeys.Ascending(expression
				.Select(ExpressionUtility.GetPropertyName)
				.ToArray());
			GetCollection().EnsureIndex(indexKeys);
		}

		#endregion
	}
}