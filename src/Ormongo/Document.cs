using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Ormongo.Internal;
using Ormongo.Plugins;

namespace Ormongo
{
	public class Document<T> : IDocument
		where T : Document<T>
	{
		#region Static

		#region Events

		public static EventHandler<DocumentEventArgs<T>> BeforeCreate;
		public static EventHandler<DocumentEventArgs<T>> AfterCreate;
		public static EventHandler<DocumentEventArgs<T>> BeforeUpdate;
		public static EventHandler<DocumentEventArgs<T>> AfterUpdate;
		public static EventHandler<DocumentEventArgs<T>> BeforeSave;
		public static EventHandler<DocumentEventArgs<T>> AfterSave;
		public static EventHandler<DocumentEventArgs<T>> BeforeDestroy;
		public static EventHandler<DocumentEventArgs<T>> AfterDestroy;

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

		public bool IsPersisted
		{
			get { return !(IsNewRecord || IsDestroyed); }
		}

		public bool IsDestroyed { get; private set; }

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

		public void Save()
		{
			OnBeforeSave();
			if (IsNewRecord)
			{
				OnBeforeCreate();
				GetCollection().Insert(this);
				OnAfterCreate();
			}
			else
			{
				OnBeforeUpdate();
				GetCollection().Save(this);
				OnAfterUpdate();
			}
			OnAfterSave();
		}

		public static T Create(T item)
		{
			item.Save();
			return item;
		}

		public static void Delete(ObjectId id)
		{
			GetCollection().Remove(GetIDQuery(id));
		}

		public static void DeleteAll()
		{
			GetCollection().RemoveAll();
		}

		public static void DestroyAll()
		{
			foreach (var item in FindAll())
				item.Destroy();
		}

		public void Delete()
		{
			GetCollection().Remove(GetIDQuery(ID));
		}

		public void Destroy()
		{
			OnBeforeDestroy();
			Delete();
			IsDestroyed = true;
			OnAfterDestroy();
		}

		#endregion

		#region Querying

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
			// Change the local value.
			ExpressionUtility.IncrementPropertyValue((T) this, expression, value);

			// Change the database value.
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

		#region Callbacks

		protected virtual void OnBeforeSave()
		{
			PluginManager.Execute(p => p.BeforeSave(this));
			if (BeforeSave != null)
				BeforeSave(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnAfterSave()
		{
			PluginManager.Execute(p => p.AfterSave(this));
			if (AfterSave != null)
				AfterSave(this, new DocumentEventArgs<T>((T) this));
		}

		protected virtual void OnBeforeCreate()
		{
			PluginManager.Execute(p => p.BeforeCreate(this));
			if (BeforeCreate != null)
				BeforeCreate(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnAfterCreate()
		{
			PluginManager.Execute(p => p.AfterCreate(this));
			if (AfterCreate != null)
				AfterCreate(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnBeforeUpdate()
		{
			PluginManager.Execute(p => p.BeforeUpdate(this));
			if (BeforeUpdate != null)
				BeforeUpdate(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnAfterUpdate()
		{
			PluginManager.Execute(p => p.AfterUpdate(this));
			if (AfterUpdate != null)
				AfterUpdate(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnBeforeDestroy()
		{
			PluginManager.Execute(p => p.BeforeDestroy(this));
			if (BeforeDestroy != null)
				BeforeDestroy(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void OnAfterDestroy()
		{
			PluginManager.Execute(p => p.AfterDestroy(this));
			if (AfterDestroy != null)
				AfterDestroy(this, new DocumentEventArgs<T>((T)this));
		}

		protected virtual void AfterFind()
		{
			PluginManager.Execute(p => p.AfterFind(this));
		}

		void IDocument.AfterFind()
		{
			AfterFind();
		}

		#endregion
	}
}