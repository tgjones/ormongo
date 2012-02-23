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
using Ormongo.Internal.Proxying;
using Ormongo.Plugins;

namespace Ormongo
{
	public class Document<T> : IDocument
		where T : Document<T>
	{
		#region Static

		#region Events

		public static EventHandler<DocumentEventArgs<T>> AfterInitialize;
		public static EventHandler<CancelDocumentEventArgs<T>> BeforeCreate;
		public static EventHandler<DocumentEventArgs<T>> AfterCreate;
		public static EventHandler<CancelDocumentEventArgs<T>> BeforeUpdate;
		public static EventHandler<DocumentEventArgs<T>> AfterUpdate;
		public static EventHandler<CancelDocumentEventArgs<T>> BeforeSave;
		public static EventHandler<DocumentEventArgs<T>> AfterSave;
		public static EventHandler<CancelDocumentEventArgs<T>> BeforeDestroy;
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

		[BsonIgnore]
		public bool IsDestroyed { get; private set; }

		internal static MongoCollection<T> GetCollection()
		{
			string collectionName = CollectionUtility.GetCollectionName(typeof(T));
			return OrmongoConfiguration.GetMongoDatabase().GetCollection<T>(collectionName);
		}

		private static IMongoQuery GetIDQuery(ObjectId id)
		{
			return Query.EQ("_id", id);
		}

		public Document()
		{
			OnAfterInitialize(new DocumentEventArgs<T>((T) this));
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
			var beforeSaveEventArgs = new CancelDocumentEventArgs<T>((T) this);
			OnBeforeSave(beforeSaveEventArgs);
			if (beforeSaveEventArgs.Cancel)
				return;

			if (IsNewRecord)
			{
				var beforeCreateEventArgs = new CancelDocumentEventArgs<T>((T) this);
				OnBeforeCreate(beforeCreateEventArgs);
				if (beforeCreateEventArgs.Cancel)
					return;

				GetCollection().Insert(this);

				OnAfterCreate(new DocumentEventArgs<T>((T) this));
			}
			else
			{
				var beforeUpdateEventArgs = new CancelDocumentEventArgs<T>((T) this);
				OnBeforeUpdate(beforeUpdateEventArgs);
				if (beforeUpdateEventArgs.Cancel)
					return;

				GetCollection().Save(this);

				OnAfterUpdate(new DocumentEventArgs<T>((T) this));
			}

			OnAfterSave(new DocumentEventArgs<T>((T) this));
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
			var beforeDestroyEventArgs = new CancelDocumentEventArgs<T>((T) this);
			OnBeforeDestroy(beforeDestroyEventArgs);
			if (beforeDestroyEventArgs.Cancel)
				return;

			Delete();
			IsDestroyed = true;

			OnAfterDestroy(new DocumentEventArgs<T>((T) this));
		}

		#endregion

		#region Querying

		public static T FindOneByID(ObjectId id)
		{
			return GetCollection().FindOneById(id);
		}

		public static TDerived FindOneByID<TDerived>(ObjectId id)
			where TDerived : T
		{
			return (TDerived) GetCollection().FindOneById(id);
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

		private void OnAfterInitialize(DocumentEventArgs<T> args)
		{
			// Not virtual because we call it from constructor.
			if (AfterInitialize != null)
				AfterInitialize(this, args);
		}

		protected virtual void OnBeforeSave(CancelDocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.BeforeSave(this));
			InvokeCancellableEvent(BeforeSave, args);
		}

		protected virtual void OnAfterSave(DocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.AfterSave(this));
			if (AfterSave != null)
				AfterSave(this, args);
		}

		protected virtual void OnBeforeCreate(CancelDocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.BeforeCreate(this));
			InvokeCancellableEvent(BeforeCreate, args);
		}

		protected virtual void OnAfterCreate(DocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.AfterCreate(this));
			if (AfterCreate != null)
				AfterCreate(this, args);
		}

		protected virtual void OnBeforeUpdate(CancelDocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.BeforeUpdate(this));
			InvokeCancellableEvent(BeforeUpdate, args);
		}

		protected virtual void OnAfterUpdate(DocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.AfterUpdate(this));
			if (AfterUpdate != null)
				AfterUpdate(this, args);
		}

		protected virtual void OnBeforeDestroy(CancelDocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.BeforeDestroy(this));
			InvokeCancellableEvent(BeforeDestroy, args);
		}

		protected virtual void OnAfterDestroy(DocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.AfterDestroy(this));
			if (AfterDestroy != null)
				AfterDestroy(this, args);
		}

		protected virtual void OnAfterFind(DocumentEventArgs<T> args)
		{
			PluginManager.Execute(p => p.AfterFind(this));
		}

		void IDocument.AfterFind()
		{
			OnAfterFind(new DocumentEventArgs<T>((T) this));
		}

		protected void InvokeCancellableEvent<TE>(EventHandler<TE> eventHandler, TE args)
			where TE : CancelDocumentEventArgs<T>
		{
			if (eventHandler == null)
				return;

			foreach (EventHandler<TE> handler in eventHandler.GetInvocationList())
			{
				handler(this, args);
				if (args.Cancel)
					return;
			}
		}

		#endregion

		#region Equality

		public static bool operator==(Document<T> left, Document<T> right)
		{
			if (ReferenceEquals(left, null))
				return ReferenceEquals(right, null);
			return left.Equals(right);
		}

		public static bool operator !=(Document<T> left, Document<T> right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
				return false;

			if (!ProxyManager.AreSameTypes(obj.GetType(), GetType()))
				return false;

			var document = obj as Document<T>;
			if (ReferenceEquals(document, null))
				return false;

			if (!IsNewRecord && !document.IsNewRecord)
				return ID == document.ID;
			
			return ReferenceEquals(this, document);
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region Lazy loading

		private readonly Dictionary<string, ObjectId> _referencesOneIDs = new Dictionary<string, ObjectId>();
		Dictionary<string, ObjectId> IDocument.ReferencesOneIDs
		{
			get { return _referencesOneIDs; }
		}

		private readonly Dictionary<string, List<ObjectId>> _referencesManyIDs = new Dictionary<string, List<ObjectId>>();
		Dictionary<string, List<ObjectId>> IDocument.ReferencesManyIDs
		{
			get { return _referencesManyIDs; }
		}

		#endregion
	}
}