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
using Ormongo.Validation;

namespace Ormongo
{
	public class Document<T> : ChangeTrackingObject<T>, IDocument, IValidatableObject
		where T : Document<T>
	{
		#region Static

		public static Func<IQueryable<T>, IQueryable<T>> DefaultScope { get; set; }
		public static List<IObserver<T>> Observers { get; private set; }
		public static List<IPlugin<T>> Plugins { get; private set; }

		static Document()
		{
			// Perform one-time initialization.
			OrmongoConfiguration.Initialize();

			// Initialize default scope.
			DefaultScope = items => items;

			Observers = new List<IObserver<T>>();
			Plugins = new List<IPlugin<T>>();

			Validators = new Dictionary<Func<T, object>, ValueValidatorBase<T>[]>();
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
			OnAfterInitialize();
			ResetChanges();
		}

		#region Persistence

		public static T Load(T newInstance)
		{
			foreach (var plugin in Plugins)
				plugin.Load(ref newInstance);
			return newInstance;
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

		public void Save()
		{
			Action finalAction = () =>
			{
				if (!OnBeforeSave())
					return;

				if (IsNewRecord)
				{
					if (!OnBeforeCreate())
						return;
					GetCollection().Insert(this);
					OnAfterCreate();
				}
				else
				{
					if (!OnBeforeUpdate())
						return;
					GetCollection().Save(this);
					OnAfterUpdate();
				}

				OnAfterSave();
			};
			foreach (var plugin in Plugins)
				plugin.Save((T) this, ref finalAction);
			finalAction();
		}

		public static T Create(T item)
		{
			item.Save();
			return item;
		}

		public static TDerived Create<TDerived>(TDerived item)
			where TDerived : T
		{
			item.Save();
			return item;
		}

		public static void Delete(ObjectId id)
		{
			Action finalAction = () => GetCollection().Remove(GetIDQuery(id));
			foreach (var plugin in Plugins)
				plugin.Delete(id, ref finalAction);
			finalAction();
		}

		public static void DeleteAll()
		{
			GetCollection().RemoveAll();
		}

		public static void DestroyAll()
		{
			foreach (var item in All())
				item.Destroy();
		}

		public void Delete()
		{
			GetCollection().Remove(GetIDQuery(ID));
		}

		public void Destroy()
		{
			if (!OnBeforeDestroy())
				return;

			Delete();
			IsDestroyed = true;

			OnAfterDestroy();
		}

		#endregion

		#region Querying

		public static T Find(ObjectId id)
		{
			Func<T> finalAction = () => GetCollection().FindOneById(id);
			foreach (var plugin in Plugins)
				plugin.Find(id, ref finalAction);
			return finalAction();
		}

		public static TDerived Find<TDerived>(ObjectId id)
			where TDerived : T
		{
			return (TDerived)Find(id);
		}

		public static IQueryable<T> Find(ObjectId[] ids)
		{
			return All().Where(d => ids.Contains(d.ID));
		}

		public static IQueryable<TDerived> Find<TDerived>(ObjectId[] ids)
			where TDerived : T
		{
			return All().Where(d => ids.Contains(d.ID)).Cast<TDerived>();
		}

		public static T Find(Expression<Func<T, bool>> predicate)
		{
			return All().SingleOrDefault(predicate);
		}

		public static TDerived Find<TDerived>(Expression<Func<TDerived, bool>> predicate)
			where TDerived : T
		{
			return All().OfType<TDerived>().SingleOrDefault(predicate);
		}

		public static IQueryable<T> Where(Expression<Func<T, bool>> predicate)
		{
			return All().Where(predicate);
		}

		public static IQueryable<T> All()
		{
			return ApplyDefaultScope(GetCollection().AsQueryable());
		}

		public static IEnumerable<T> Near<TProperty>(Expression<Func<T, TProperty>> expression,
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

		private void OnAfterInitialize()
		{
			// Not virtual because we call it from constructor.
			ExecuteObservers(o => o.AfterInitialize((T) this));
		}

		protected virtual bool OnBeforeSave()
		{
			EmbeddedDocumentUtility.UpdateParentReferences(this);
			return ExecuteCancellableObservers(o => o.BeforeSave((T)this));
		}

		protected virtual void OnAfterSave()
		{
			ResetChanges();
			ExecuteObservers(o => o.AfterSave((T)this));
		}

		protected virtual bool OnBeforeCreate()
		{
			return ExecuteCancellableObservers(o => o.BeforeCreate((T)this));
		}

		protected virtual void OnAfterCreate()
		{
			ExecuteObservers(o => o.AfterCreate((T)this));
		}

		protected virtual bool OnBeforeUpdate()
		{
			return ExecuteCancellableObservers(o => o.BeforeUpdate((T)this));
		}

		protected virtual void OnAfterUpdate()
		{
			ExecuteObservers(o => o.AfterUpdate((T)this));
		}

		protected virtual bool OnBeforeDestroy()
		{
			return ExecuteCancellableObservers(o => o.BeforeDestroy((T)this));
		}

		protected virtual void OnAfterDestroy()
		{
			ExecuteObservers(o => o.AfterDestroy((T)this));
		}

		protected virtual void OnAfterFind()
		{
			EmbeddedDocumentUtility.UpdateParentReferences(this);
			ResetChanges();
			ExecuteObservers(o => o.AfterFind((T)this));
		}

		void IDocument.AfterFind()
		{
			OnAfterFind();
		}

		private void ExecuteObservers(Action<IObserver<T>> callback)
		{
			ExecuteObservers<IObserver<T>>(callback);
		}

		protected void ExecuteObservers<TObserver>(Action<TObserver> callback)
		{
			foreach (var observer in Observers.OfType<TObserver>())
				callback(observer);
		}

		private bool ExecuteCancellableObservers(Func<IObserver<T>, bool> callback)
		{
			return ExecuteCancellableObservers<IObserver<T>>(callback);
		}

		protected bool ExecuteCancellableObservers<TObserver>(Func<TObserver, bool> callback)
		{
			return Observers.OfType<TObserver>().All(callback);
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

		#region Validation

		private static readonly Dictionary<Func<T, object>, ValueValidatorBase<T>[]> Validators;

		public bool IsValid
		{
			get { return ValidationUtility.GetIsValid(this); }
		}

		protected static void Validates<TProperty>(Expression<Func<T, TProperty>> propertyExpression, params ValueValidatorBase<T>[] validators)
		{
			foreach (var validator in validators)
				validator.Initialize(propertyExpression);
			Validators.Add(x => propertyExpression.Compile()(x), validators);
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var results = new List<ValidationResult>();
			results.AddRange(ValidationUtility.Validate(this, Validators, validationContext));
			foreach (IValidatableObject embeddedDocument in EmbeddedDocumentUtility.GetEmbeddedDocuments(this))
				results.AddRange(embeddedDocument.Validate(validationContext));
			return results;
		}

		#endregion
	}
}