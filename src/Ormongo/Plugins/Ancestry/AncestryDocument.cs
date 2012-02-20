using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ormongo.Plugins.Ancestry
{
	public abstract class AncestryDocument<T> : Document<T>
		where T : AncestryDocument<T>
	{
		private AncestryProxy<T> _ancestry;

		protected AncestryProxy<T> Ancestry
		{
			get { return _ancestry ?? (_ancestry = new AncestryProxy<T>((T) this)); }
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return Ancestry.AncestorIDs; }
		}

		public IQueryable<T> Ancestors
		{
			get { return Ancestry.Ancestors; }
		}

		public IEnumerable<ObjectId> AncestorsAndSelfIDs
		{
			get { return Ancestry.AncestorsAndSelfIDs; }
		}

		public IQueryable<T> AncestorsAndSelf
		{
			get { return Ancestry.AncestorsAndSelf; }
		}

		public int Depth
		{
			get { return Ancestry.Depth; }
		}

		#endregion

		#region Parent

		[BsonIgnore]
		public T Parent
		{
			get { return Ancestry.Parent; }
			set { Ancestry.Parent = value; }
		}

		[BsonIgnore]
		public ObjectId ParentID
		{
			get { return Ancestry.ParentID; }
			set { Ancestry.ParentID = value; }
		}

		#endregion

		#region Root

		public ObjectId RootID
		{
			get { return Ancestry.RootID; }
		}

		public T Root
		{
			get { return Ancestry.Root; }
		}

		public bool IsRoot
		{
			get { return Ancestry.IsRoot; }
		}

		#endregion

		#region Children

		public Relation<T> Children
		{
			get { return Ancestry.Children; }
		}

		public IEnumerable<ObjectId> ChildIDs
		{
			get { return Ancestry.ChildIDs; }
		}

		public bool HasChildren
		{
			get { return Ancestry.HasChildren; }
		}

		public bool IsChildless
		{
			get { return Ancestry.IsChildless; }
		}

		#endregion

		#region Siblings

		public IQueryable<T> SiblingsAndSelf
		{
			get { return Ancestry.SiblingsAndSelf; }
		}

		public IQueryable<T> Siblings
		{
			get { return Ancestry.Siblings; }
		}

		public IEnumerable<ObjectId> SiblingIDs
		{
			get { return Ancestry.SiblingIDs; }
		}

		public bool HasSiblings
		{
			get { return Ancestry.HasSiblings; }
		}

		public bool IsOnlyChild
		{
			get { return Ancestry.IsOnlyChild; }
		}

		#endregion

		#region Descendants

		public IQueryable<T> DescendantsAndSelf
		{
			get { return Ancestry.DescendantsAndSelf; }
		}

		public IEnumerable<ObjectId> DescendantsAndSelfIDs
		{
			get { return Ancestry.DescendantsAndSelfIDs; }
		}

		public IQueryable<T> Descendants
		{
			get { return Ancestry.Descendants; }
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Ancestry.DescendantIDs; }
		}

		#endregion
	}
}