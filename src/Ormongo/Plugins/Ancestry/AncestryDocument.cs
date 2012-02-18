using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ormongo.Plugins.Ancestry
{
	public class AncestryDocument<T> : Document<T>
		where T : AncestryDocument<T>
	{
		[ScaffoldColumn(false)]
		public string Ancestry { get; set; }

		#region Querying

		public static IQueryable<T> Roots
		{
			get { return Find(i => i.Ancestry == null); }
		}
		
		// scope :ancestors_of, lambda { |object| where(to_node(object).ancestor_conditions) }

		public static IQueryable<T> AncestorsOf(T item)
		{
			return Find(i => i.AncestorIDs.Contains(i.ID));
		}

		#endregion

		/// <summary>
		/// The ancestry value for this record's children
		/// </summary>
		public string ChildAncestry
		{
			get
			{
				// New records cannot have children
				if (IsNewRecord)
					throw new InvalidOperationException("No child ancestry for new record. Save record before performing tree operations.");

				return (string.IsNullOrEmpty(Ancestry))
					? ID.ToString()
					: string.Format("{0}/{1}", Ancestry, ID);
			}
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return (string.IsNullOrEmpty(Ancestry)) ? new List<ObjectId>() : Ancestry.Split('/').Select(ObjectId.Parse); }
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

		public IQueryable<T> Children
		{
			get { return Find(d => d.Ancestry == ChildAncestry); }
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
			get { return Find(d => d.Ancestry == Ancestry); }
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
			get { return Find(d => d.Ancestry.StartsWith(ChildAncestry + "/") || d.Ancestry == ChildAncestry); }
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Subtree

		public IQueryable<T> Subtree
		{
			get { return Find(d => d.ID == ID || d.Ancestry.StartsWith(ChildAncestry + "/") || d.Ancestry == ChildAncestry); }
		}

		public IEnumerable<ObjectId> SubtreeIDs
		{
			get { return Subtree.Select(d => d.ID); }
		}

		#endregion
	}
}