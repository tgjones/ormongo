using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Ormongo.Internal;
using Ormongo.Internal.Serialization;
using Ormongo.Plugins;

namespace Ormongo
{
	public class Document<T>
		where T : Document<T>
	{
		#region Static

		#region Events

		public static EventHandler<DocumentEventArgs<T>> Created;
		public static EventHandler<DocumentDeletingEventArgs> Deleting;
		public static EventHandler<DocumentEventArgs<T>> Saving;
		public static EventHandler<DocumentEventArgs<T>> Saved;

		#endregion

		public static Func<IQueryable<T>, IQueryable<T>> DefaultScope { get; set; }

		static Document()
		{
			// Perform one-time initialization.
			OrmongoConfiguration.Initialize();

			// Initialize plugins.
			PluginManager.Execute(p => p.Initialize(typeof(T)));

			// Initialize default scope.
			DefaultScope = items => items;
		}

		#endregion

		[BsonId, ScaffoldColumn(false)]
		public ObjectId ID { get; set; }

		public bool IsNewRecord
		{
			get { return ID == ObjectId.Empty; }
		}

		/// <summary>
		/// Useful for temporarily storing data that doesn't need to be persisted to the database.
		/// Some plugins use this to maintain state within the document.
		/// </summary>
		[BsonIgnore]
		public DataDictionary TransientData { get; private set; }

		/// <summary>
		/// A key/value collection of arbitrary data that doesn't have a strongly-typed
		/// property accessor.
		/// </summary>
		public DataDictionary ExtraData { get; set; }

		public Document()
		{
			TransientData = new DataDictionary();
			ExtraData = new DataDictionary();
		}

		internal static MongoCollection<T> GetCollection()
		{
			string collectionName = typeof (T).Name;
			return OrmongoConfiguration.GetMongoDatabase().GetCollection<T>(collectionName);
		}

		private static IMongoQuery GetIDQuery(ObjectId id)
		{
			return Query.EQ("_id", id);
		}

		#region Persistence

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

		protected virtual void OnSaving(object sender, DocumentEventArgs<T> args)
		{
			if (Saving != null)
				Saving(sender, args);
		}

		protected virtual void OnSaved(object sender, DocumentEventArgs<T> args)
		{
			if (Saved != null)
				Saved(sender, args);
		}

		public void Save()
		{
			OnSaving(this, new DocumentEventArgs<T>((T) this));
			PluginManager.Execute(p => p.BeforeSave(this));
			GetCollection().Save(this);
			OnSaved(this, new DocumentEventArgs<T>((T) this));
			PluginManager.Execute(p => p.AfterSave(this));
		}

		public static T Create(T item)
		{
			item.Save();
			item.OnCreated(null, new DocumentEventArgs<T>(item));
			return item;
		}

		protected virtual void OnCreated(object sender, DocumentEventArgs<T> args)
		{
			if (Created != null)
				Created(sender, args);
		}

		public static void Delete(ObjectId id)
		{
			OnDeleting(null, new DocumentDeletingEventArgs(id));
			GetCollection().Remove(GetIDQuery(id));
		}

		private static void OnDeleting(object sender, DocumentDeletingEventArgs args)
		{
			if (Deleting != null)
				Deleting(sender, args);
		}

		public static void DeleteAll()
		{
			GetCollection().RemoveAll();
		}

		public void Destroy()
		{
			PluginManager.Execute(p => p.BeforeDestroy(this));
			Delete(ID);
			PluginManager.Execute(p => p.AfterDestroy(this));
		}

		#endregion

		#region Querying

		public static IQueryable<T> FindNative(IMongoQuery query)
		{
			return ApplyDefaultScope(GetCollection().Find(query).AsQueryable());
		}

		public static T FindOneByID(ObjectId id)
		{
			return GetCollection().FindOneById(id);
		}

		public static T FindOne(Expression<Func<T, bool>> predicate)
		{
			return FindAll().SingleOrDefault(predicate);
		}

		public static IQueryable<T> Find(Expression<Func<T, bool>> predicate)
		{
			return FindAll().Where(predicate);
		}

		public static IQueryable<T> FindAll()
		{
			return ApplyDefaultScope(GetCollection().AsQueryable());
		}

		public static IEnumerable<T> FindNear<TProperty>(Expression<Func<T, TProperty>> expression,
			double x, double y, double maxDistance, int limit)
		{
			var query = Query.Near(ExpressionUtility.GetPropertyName(expression), x, y, maxDistance);
			var cursor = GetCollection().Find(query);
			cursor.Limit = limit;
			return cursor;
		}

		private static IQueryable<T> ApplyDefaultScope(IQueryable<T> items)
		{
			return DefaultScope(items);
		}

		#endregion

		#region Indexing

		public static void EnsureIndex<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			EnsureIndex(expression, false);
		}

		public static void EnsureIndex<TProperty>(Expression<Func<T, TProperty>> expression, bool unique)
		{
			IMongoIndexKeys indexKeys = IndexKeys.Ascending(ExpressionUtility.GetPropertyName(expression));
			EnsureIndexInternal(indexKeys, unique);
		}

		public static void EnsureIndex<TProperty1, TProperty2>(Expression<Func<T, TProperty1>> expression1, Expression<Func<T, TProperty2>> expression2)
		{
			EnsureIndex(expression1, expression2, false);
		}

		public static void EnsureIndex<TProperty1, TProperty2>(Expression<Func<T, TProperty1>> expression1, Expression<Func<T, TProperty2>> expression2, bool unique)
		{
			IMongoIndexKeys indexKeys = IndexKeys.Ascending(ExpressionUtility.GetPropertyName(expression1), ExpressionUtility.GetPropertyName(expression2));
			EnsureIndexInternal(indexKeys, unique);
		}

		public static void EnsureIndex<TProperty1, TProperty2, TProperty3>(Expression<Func<T, TProperty1>> expression1, Expression<Func<T, TProperty2>> expression2, Expression<Func<T, TProperty3>> expression3, bool unique)
		{
			IMongoIndexKeys indexKeys = IndexKeys.Ascending(ExpressionUtility.GetPropertyName(expression1), ExpressionUtility.GetPropertyName(expression2), ExpressionUtility.GetPropertyName(expression3));
			EnsureIndexInternal(indexKeys, unique);
		}

		private static void EnsureIndexInternal(IMongoIndexKeys indexKeys, bool unique)
		{
			GetCollection().EnsureIndex(indexKeys, IndexOptions.SetUnique(unique));
		}


		public static void EnsureGeoSpatialIndex<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var indexKeys = IndexKeys.GeoSpatial(ExpressionUtility.GetPropertyName(expression));
			GetCollection().EnsureIndex(indexKeys);
		}

		#endregion

		#region Atomic

		public static void Push<TProperty>(ObjectId id, Expression<Func<T, List<TProperty>>> expression, TProperty value)
		{
			UpdateBuilder update = Update.PushWrapped(ExpressionUtility.GetPropertyName(expression), value);
			GetCollection().Update(GetIDQuery(id), update);
		}

		public static void Pull<TProperty>(ObjectId id, Expression<Func<T, List<TProperty>>> expression, Expression<Func<TProperty, bool>> match)
		{
			string propertyName;
			object value;
			ExpressionUtility.GetPropertyNameAndValue(match, out propertyName, out value);

			UpdateBuilder update = Update.Pull(ExpressionUtility.GetPropertyName(expression),
				Query.EQ(propertyName, BsonValue.Create(value)));
			GetCollection().Update(GetIDQuery(id), update);
		}

		public void Inc<TProperty>(Expression<Func<T, TProperty>> expression, int value)
		{
			UpdateBuilder update = Update.Inc(ExpressionUtility.GetPropertyName(expression), value);
			GetCollection().Update(GetIDQuery(ID), update);
		}

		#endregion

		#region Associations

		public void UpdateAssociations()
		{
			EmbeddedDocumentUtility.UpdateParentReferences(this);
		}

		#endregion
	}
}