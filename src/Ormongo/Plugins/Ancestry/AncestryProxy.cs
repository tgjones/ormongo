using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;

namespace Ormongo.Plugins.Ancestry
{
	public class AncestryProxy<T> : IAncestryProxy
		where T : Document<T>
	{
		private const string AncestryKey = "Ancestry";

		private readonly T _instance;
		private string _ancestryDataWas;
		private bool _ancestryDataChanged;

		#region Static

		private static AncestryProxy<T> GetAncestryProxy(T instance)
		{
			return (AncestryProxy<T>) ((IHasAncestry) instance).AncestryProxy;
		}

		//public static IQueryable<T> Roots
		//{
		//    get { return Document<T>.Find(i => i.AncestryData == null); }
		//}
		
		// scope :ancestors_of, lambda { |object| where(to_node(object).ancestor_conditions) }

		//public static IQueryable<T> AncestorsOf(T item)
		//{
		//    return Document<T>.Find(i => item.Ancestry.AncestorIDs.Contains(i.ID));
		//}

		#endregion

		#region Constructor

		public AncestryProxy(T instance)
		{
			_instance = instance;
			_ancestryDataWas = Ancestry;
		}

		#endregion

		#region Instance

		public string Ancestry
		{
			get
			{
				BsonValue value;
				if (_instance.ExtraData.TryGetValue(AncestryKey, out value))
					return value.AsString;
				return null;
			}
			set { _instance.ExtraData[AncestryKey] = value; }
		}

		void IAncestryProxy.UpdateDescendantsWithNewAncestry()
		{
			// Skip this if callbacks are disabled.
			if (AncestryCallbacksDisabled)
				return;

			// Skip this if it's a new record or ancestry wasn't updated.
			if (_instance.IsNewRecord || !_ancestryDataChanged)
				return;

			// For each descendant...
			foreach (var descendant in Descendants)
			{
				// Replace old ancestry with new ancestry.
				GetAncestryProxy(descendant).WithoutAncestryCallbacks(() =>
				{
					string forReplace = (string.IsNullOrEmpty(Ancestry))
						? _instance.ID.ToString()
						: string.Format("{0}/{1}", Ancestry, _instance.ID);
					string newAncestry = Regex.Replace((string)descendant.ExtraData[AncestryKey], "^" + ChildAncestry, forReplace);
					descendant.ExtraData[AncestryKey] = newAncestry;
					descendant.Save();
				});
			}
		}

		void IAncestryProxy.ResetChangedFields()
		{
			_ancestryDataWas = Ancestry;
		}

		private void UpdateAncestryData(string newValue)
		{
			Ancestry = newValue;
			_ancestryDataChanged = true;
		}

		/// <summary>
		/// The ancestry value for this record's children
		/// </summary>
		internal string ChildAncestry
		{
			get
			{
				// New records cannot have children
				if (_instance.IsNewRecord)
					throw new InvalidOperationException("No child ancestry for new record. Save record before performing tree operations.");

				return (string.IsNullOrEmpty(_ancestryDataWas))
					? _instance.ID.ToString()
					: string.Format("{0}/{1}", _ancestryDataWas, _instance.ID);
			}
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return (string.IsNullOrEmpty(Ancestry)) ? new List<ObjectId>() : Ancestry.Split('/').Select(ObjectId.Parse); }
		}

		public IQueryable<T> Ancestors
		{
			get { return Document<T>.Find(d => AncestorIDs.Contains(d.ID)); }
		}

		public IEnumerable<ObjectId> PathIDs
		{
			get { return AncestorIDs.Union(new[] { _instance.ID }); }
		}

		public IQueryable<T> Path
		{
			get { return Document<T>.Find(d => PathIDs.Contains(d.ID)); }
		}

		public int Depth
		{
			get { return AncestorIDs.Count(); }
		}

		#endregion

		#region Parent

		[BsonIgnore]
		public T Parent
		{
			get { return (ParentID == ObjectId.Empty) ? null : Document<T>.FindOneByID(ParentID); }
			set { UpdateAncestryData((value == null) ? null : GetAncestryProxy(value).ChildAncestry); }
		}

		[BsonIgnore]
		public ObjectId ParentID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.Last() : ObjectId.Empty; }
			set { Parent = (value == ObjectId.Empty) ? null : Document<T>.FindOneByID(value); }
		}

		#endregion

		#region Root

		public ObjectId RootID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.First() : _instance.ID; }
		}

		public T Root
		{
			get { return (RootID == _instance.ID) ? _instance : Document<T>.FindOneByID(RootID); }
		}

		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(Ancestry); }
		}

		#endregion

		#region Children

		public IQueryable<T> Children
		{
			get { return Document<T>.Find(d => d.ExtraData[AncestryKey] == ChildAncestry); }
		}

		public IEnumerable<ObjectId> ChildIDs
		{
			get { return Children.Select(d => d.ID); }
		}

		public bool HasChildren
		{
			get { return Children.Any(); }
		}

		public bool IsChildless
		{
			get { return !HasChildren; }
		}

		#endregion

		#region Siblings

		public IQueryable<T> Siblings
		{
			get { return Document<T>.Find(d => d.ExtraData[AncestryKey] == Ancestry); }
		}

		public IEnumerable<ObjectId> SiblingIDs
		{
			get { return Siblings.Select(d => d.ID); }
		}

		public bool HasSiblings
		{
			get { return Siblings.Count() > 1; }
		}

		public bool IsOnlyChild
		{
			get { return !HasSiblings; }
		}

		#endregion

		#region Descendants

		public IQueryable<T> Descendants
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.Matches("ExtraProperties." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ("ExtraProperties." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Subtree

		public IQueryable<T> Subtree
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.EQ("_id", _instance.ID),
					Query.Matches("ExtraProperties." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ("ExtraProperties." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> SubtreeIDs
		{
			get { return Subtree.Select(d => d.ID); }
		}

		#endregion

		#region Callback disabling

		private bool _disableAncestryCallbacks;

		public void WithoutAncestryCallbacks(Action callback)
		{
			_disableAncestryCallbacks = true;
			callback();
			_disableAncestryCallbacks = false;
		}

		public bool AncestryCallbacksDisabled
		{
			get { return _disableAncestryCallbacks; }
		}

		#endregion

		#endregion
	}
}