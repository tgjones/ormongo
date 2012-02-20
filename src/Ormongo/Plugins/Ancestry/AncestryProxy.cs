using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Ormongo.Internal;

namespace Ormongo.Plugins.Ancestry
{
	public class AncestryProxy<T> : IAncestryProxy
		where T : Document<T>
	{
		internal const string AncestryKey = "Ancestry";
		private const string AncestryWasKey = "AncestryWas";
		private const string AncestryChangedKey = "AncestryChanged";
		private const string DisableAncestryCallbacksKey = "DisableAncestryCallbacks";

		private readonly T _instance;

		private bool AncestryChanged
		{
			get { return _instance.TransientData.SafeGet<bool>(AncestryChangedKey); }
			set { _instance.TransientData.SafeSet(AncestryChangedKey, value); }
		}

		private string AncestryWas
		{
			get { return _instance.TransientData.SafeGet<string>(AncestryWasKey); }
			set { _instance.TransientData.SafeSet(AncestryWasKey, value); }
		}

		#region Static

		private static AncestryProxy<T> GetAncestryProxy(T instance)
		{
			return new AncestryProxy<T>(instance);
		}

		#endregion

		#region Constructor

		public AncestryProxy(T instance)
		{
			_instance = instance;
			AncestryWas = Ancestry;
		}

		#endregion

		#region Instance

		public string Ancestry
		{
			get { return _instance.ExtraData.SafeGet<string>(AncestryKey); }
			set
			{
				_instance.ExtraData.SafeSet(AncestryKey, value);
				AncestryChanged = true;
			}
		}

		void IAncestryProxy.ResetChangedFields()
		{
			AncestryWas = Ancestry;
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

				return (String.IsNullOrEmpty(AncestryWas))
					? _instance.ID.ToString()
					: String.Format("{0}/{1}", AncestryWas, _instance.ID);
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

		public IEnumerable<ObjectId> AncestorsAndSelfIDs
		{
			get { return AncestorIDs.Union(new[] { _instance.ID }); }
		}

		public IQueryable<T> AncestorsAndSelf
		{
			get { return Document<T>.Find(d => AncestorsAndSelfIDs.Contains(d.ID)); }
		}

		public int Depth
		{
			get { return AncestorIDs.Count(); }
		}

		#endregion

		#region Parent

		public T Parent
		{
			get { return (ParentID == ObjectId.Empty) ? null : Document<T>.FindOneByID(ParentID); }
			set { Ancestry = (value == null) ? null : GetAncestryProxy(value).ChildAncestry; }
		}

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

		public Relation<T> Children
		{
			get
			{
				return new Relation<T>(
					Document<T>.Find(d => d.ExtraData[AncestryKey] == ChildAncestry),
					d => d.ExtraData[AncestryKey] = ChildAncestry);
			}
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

		public IQueryable<T> SiblingsAndSelf
		{
			get { return Document<T>.Find(d => d.ExtraData[AncestryKey] == Ancestry); }
		}

		public IQueryable<T> Siblings
		{
			get { return SiblingsAndSelf.Where(d => d.ID != _instance.ID); }
		}

		public IEnumerable<ObjectId> SiblingIDs
		{
			get { return Siblings.Select(d => d.ID); }
		}

		public bool HasSiblings
		{
			get { return Siblings.Any(); }
		}

		public bool IsOnlyChild
		{
			get { return !HasSiblings; }
		}

		#endregion

		#region Descendants

		private static string GetExtraDataPropertyName()
		{
			return ExpressionUtility.GetPropertyName<T, DataDictionary>(d => d.ExtraData);
		}

		public IQueryable<T> DescendantsAndSelf
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.EQ("_id", _instance.ID),
					Query.Matches(GetExtraDataPropertyName() + "." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ(GetExtraDataPropertyName() + "." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> DescendantsAndSelfIDs
		{
			get { return DescendantsAndSelf.Select(d => d.ID); }
		}

		public IQueryable<T> Descendants
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.Matches(GetExtraDataPropertyName() + "." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ(GetExtraDataPropertyName() + "." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Callbacks

		private void WithoutAncestryCallbacks(Action callback)
		{
			_instance.TransientData.SafeSet(DisableAncestryCallbacksKey, true);
			callback();
			_instance.TransientData.SafeSet(DisableAncestryCallbacksKey, false);
		}

		void IAncestryProxy.UpdateDescendantsWithNewAncestry()
		{
			// Skip this if callbacks are disabled.
			if (_instance.TransientData.SafeGet<bool>(DisableAncestryCallbacksKey))
				return;

			// Skip this if it's a new record or ancestry wasn't updated.
			if (_instance.IsNewRecord || !_instance.TransientData.SafeGet<bool>(AncestryChangedKey))
				return;

			// For each descendant...
			foreach (var descendant in Descendants)
			{
				// Replace old ancestry with new ancestry.
				GetAncestryProxy(descendant).WithoutAncestryCallbacks(() =>
				{
					string forReplace = (String.IsNullOrEmpty(Ancestry))
						? _instance.ID.ToString()
						: String.Format("{0}/{1}", Ancestry, _instance.ID);
					string newAncestry = Regex.Replace((string) descendant.ExtraData[AncestryKey], "^" + ChildAncestry, forReplace);
					descendant.ExtraData[AncestryKey] = newAncestry;
					descendant.Save();
				});
			}
		}

		void IAncestryProxy.ApplyOrphanStrategy(OrphanStrategy orphanStrategy)
		{
			// Skip this if callbacks are disabled.
			if (_instance.TransientData.SafeGet<bool>(DisableAncestryCallbacksKey))
				return;

			// Skip this if it's a new record.
			if (_instance.IsNewRecord)
				return;

			switch (orphanStrategy)
			{
				case OrphanStrategy.Destroy:
					foreach (var descendant in Descendants)
						GetAncestryProxy(descendant).WithoutAncestryCallbacks(() => descendant.Destroy());
					break;
				case OrphanStrategy.Rootify:
					foreach (var descendant in Descendants)
					{
						var descendantProxy = GetAncestryProxy(descendant);
						descendantProxy.WithoutAncestryCallbacks(() =>
						{
							string val = null;
							if (descendantProxy.Ancestry != ChildAncestry)
								val = Regex.Replace(descendantProxy.Ancestry, "^" + ChildAncestry + "/", string.Empty);
							descendantProxy.Ancestry = val;
							descendant.Save();
						});
					}
					break;
				case OrphanStrategy.Restrict:
					if (HasChildren)
						throw new InvalidOperationException("Cannot delete record because it has descendants");
					break;
				default:
					throw new ArgumentOutOfRangeException("orphanStrategy");
			}

		}

		#endregion

		#endregion
	}
}