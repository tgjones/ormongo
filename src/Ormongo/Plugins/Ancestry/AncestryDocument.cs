using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ormongo.Plugins.Ancestry
{
	public abstract class AncestryDocument<T> : Document<T>
		where T : AncestryDocument<T>
	{
		#region Static configuration

		/// <summary>
		/// Configures what to do with children of a node that is destroyed. Defaults to Destroy.
		/// </summary>
		public static OrphanStrategy OrphanStrategy { get; set; }

		/// <summary>
		/// Cache the depth of each node in the ExtraData.AncestryDepth field. Defaults to false.
		/// </summary>
		public static bool CacheDepth { get; set; }

		#endregion

		#region Instance

		private string _ancestry;
		protected string AncestryWas { get; private set; }
		protected bool AncestryChanged { get; private set; }

		public string Ancestry
		{
			get { return _ancestry; }
			set
			{
				_ancestry = value;
				AncestryChanged = true;
			}
		}

		/// <summary>
		/// The ancestry value for this record's children
		/// </summary>
		internal string ChildAncestry
		{
			get
			{
				// New records cannot have children
				if (IsNewRecord)
					throw new InvalidOperationException("No child ancestry for new record. Save record before performing tree operations.");

				return (string.IsNullOrEmpty(AncestryWas))
					? ID.ToString()
					: String.Format("{0}/{1}", AncestryWas, ID);
			}
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return (string.IsNullOrEmpty(Ancestry)) ? new List<ObjectId>() : Ancestry.Split('/').Select(ObjectId.Parse); }
		}

		public IQueryable<T> Ancestors
		{
			get { return Find(d => AncestorIDs.Contains(d.ID)); }
		}

		public IEnumerable<ObjectId> AncestorsAndSelfIDs
		{
			get { return AncestorIDs.Union(new[] { ID }); }
		}

		public IQueryable<T> AncestorsAndSelf
		{
			get { return Find(d => AncestorsAndSelfIDs.Contains(d.ID)); }
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
			get { return (ParentID == ObjectId.Empty) ? null : FindOneByID(ParentID); }
			set { Ancestry = (value == null) ? null : value.ChildAncestry; }
		}

		[BsonIgnore]
		public ObjectId ParentID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.Last() : ObjectId.Empty; }
			set { Parent = (value == ObjectId.Empty) ? null : FindOneByID(value); }
		}

		#endregion

		#region Root

		public ObjectId RootID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.First() : ID; }
		}

		public T Root
		{
			get { return (RootID == ID) ? (T) this : FindOneByID(RootID); }
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
					Find(d => d.Ancestry == ChildAncestry),
					d => d.Ancestry = ChildAncestry);
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
			get { return Find(d => d.Ancestry == Ancestry); }
		}

		public IQueryable<T> Siblings
		{
			get { return SiblingsAndSelf.Where(d => d.ID != ID); }
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

		/// <summary>
		/// Is this document a sibling of the other document?
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSiblingOf(T other)
		{
			return ParentID == other.ParentID;
		}

		#endregion

		#region Descendants

		public IEnumerable<T> DescendantsAndSelf
		{
			get { return Find(d => d.ID == ID || d.Ancestry.StartsWith(ChildAncestry) || d.Ancestry == ChildAncestry); }
		}

		public IEnumerable<ObjectId> DescendantsAndSelfIDs
		{
			get { return DescendantsAndSelf.Select(d => d.ID); }
		}

		public IEnumerable<T> Descendants
		{
			get { return Find(d => d.Ancestry.StartsWith(ChildAncestry) || d.Ancestry == ChildAncestry); }
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Callbacks

		protected override void AfterFind()
		{
			AncestryWas = _ancestry;
			base.AfterFind();
		}

		protected override void OnSaving(object sender, DocumentEventArgs<T> args)
		{
			UpdateDescendantsWithNewAncestry();
			base.OnSaving(sender, args);
		}

		protected override void OnSaved(object sender, DocumentEventArgs<T> args)
		{
			AncestryWas = _ancestry;
			AncestryChanged = false;
			base.OnSaved(sender, args);
		}

		protected override void BeforeDestroy()
		{
			ApplyOrphanStrategy();
			base.BeforeDestroy();
		}

		private bool _disableAncestryCallbacks;
		private void WithoutAncestryCallbacks(Action callback)
		{
			_disableAncestryCallbacks = true;
			callback();
			_disableAncestryCallbacks = false;
		}

		private void UpdateDescendantsWithNewAncestry()
		{
			// Skip this if callbacks are disabled.
			if (_disableAncestryCallbacks)
				return;

			// Skip this if it's a new record or ancestry wasn't updated.
			if (IsNewRecord || !AncestryChanged)
				return;

			// For each descendant...
			foreach (var descendant in Descendants)
			{
				// Replace old ancestry with new ancestry.
				descendant.WithoutAncestryCallbacks(() =>
				{
					string forReplace = (String.IsNullOrEmpty(Ancestry))
						? ID.ToString()
						: String.Format("{0}/{1}", Ancestry, ID);
					string newAncestry = Regex.Replace(descendant.Ancestry, "^" + ChildAncestry, forReplace);
					descendant.Ancestry = newAncestry;
					descendant.Save();
				});
			}
		}

		private void ApplyOrphanStrategy()
		{
			// Skip this if callbacks are disabled.
			if (_disableAncestryCallbacks)
				return;

			// Skip this if it's a new record.
			if (IsNewRecord)
				return;

			switch (OrphanStrategy)
			{
				case OrphanStrategy.Destroy:
					foreach (var descendant in Descendants)
						descendant.WithoutAncestryCallbacks(() => descendant.Destroy());
					break;
				case OrphanStrategy.Rootify:
					foreach (var descendant in Descendants)
					{
						descendant.WithoutAncestryCallbacks(() =>
						{
							string val = null;
							if (descendant.Ancestry != ChildAncestry)
								val = Regex.Replace(descendant.Ancestry, "^" + ChildAncestry + "/", string.Empty);
							descendant.Ancestry = val;
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