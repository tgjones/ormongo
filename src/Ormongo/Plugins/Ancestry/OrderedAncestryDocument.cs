using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public abstract class OrderedAncestryDocument<T> : AncestryDocument<T>
		where T : OrderedAncestryDocument<T>
	{
		#region Static

		static OrderedAncestryDocument()
		{
			DefaultScope = items => items.OrderBy(d => d.Position);
		}

		#endregion

		private int _position, _positionWas;
		private bool _positionChanged;

		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				_positionChanged = true;
			}
		}

		/// <summary>
		/// Returns siblings below the current document - i.e. siblings with
		/// a position greater than this document's position.
		/// </summary>
		public IQueryable<T> LowerSiblings
		{
			get { return Siblings.Where(d => d.Position > Position); }
		}

		/// <summary>
		/// Returns siblings above the current document - i.e. siblings with
		/// a position less than than this document's position.
		/// </summary>
		public IQueryable<T> HigherSiblings
		{
			get { return Siblings.Where(d => d.Position < Position); }
		}

		/// <summary>
		/// Gets the lowest sibling (could be this)
		/// </summary>
		public T LowestSibling
		{
			get { return SiblingsAndSelf.ToList().Last(); }
		}

		/// <summary>
		/// Gets the highest sibling (could be this)
		/// </summary>
		public T HighestSibling
		{
			get { return SiblingsAndSelf.First(); }
		}

		/// <summary>
		/// Is this the highest sibling?
		/// </summary>
		public bool AtTop
		{
			get { return !HigherSiblings.Any(); }
		}

		/// <summary>
		/// Is this the lowest sibling?
		/// </summary>
		public bool AtBottom
		{
			get { return !LowerSiblings.Any(); }
		}

		/// <summary>
		/// Moves this node above all its siblings.
		/// </summary>
		public void MoveToTop()
		{
			if (AtTop)
				return;
			MoveAbove(HighestSibling);
		}

		/// <summary>
		/// Moves this node below all its siblings.
		/// </summary>
		public void MoveToBottom()
		{
			if (AtBottom)
				return;
			MoveBelow(HighestSibling);
		}

		/// <summary>
		/// Moves this node above the specified node. 
		/// Changes the node's parent if necessary.
		/// </summary>
		/// <param name="other"></param>
		public void MoveAbove(T other)
		{
			if (!IsSiblingOf(other))
			{
				ParentID = other.ParentID;
				Save();
			}

			if (Position > other.Position)
			{
				int newPosition = other.Position;
				foreach (var sibling in other.LowerSiblings.Where(d => d.Position < Position))
					sibling.Inc(s => s.Position, 1);
				other.Inc(s => s.Position, 1);
				Position = newPosition;
				Save();
			}
			else
			{
				int newPosition = other.Position - 1;
				foreach (var sibling in other.HigherSiblings.Where(d => d.Position > Position))
					sibling.Inc(s => s.Position, -1);
				Position = newPosition;
				Save();
			}
		}

		/// <summary>
		/// Moves this node below the specified node.
		/// Changes the node's parent if necessary.
		/// </summary>
		/// <param name="other"></param>
		public void MoveBelow(T other)
		{
			if (!IsSiblingOf(other))
			{
				ParentID = other.ParentID;
				Save();
			}

			if (Position > other.Position)
			{
				int newPosition = other.Position + 1;
				foreach (var sibling in other.LowerSiblings.Where(d => d.Position < Position))
					sibling.Inc(s => s.Position, 1);
				Position = newPosition;
				Save();
			}
			else
			{
				int newPosition = other.Position;
				foreach (var sibling in other.HigherSiblings.Where(d => d.Position > Position))
					sibling.Inc(s => s.Position, -1);
				other.Inc(s => s.Position, 1);
				Position = newPosition;
				Save();
			}
		}

		#region Callbacks

		protected override void AfterFind()
		{
			_positionWas = _position;
			base.AfterFind();
		}

		protected override void OnBeforeSave()
		{
			base.OnBeforeSave();
			AssignDefaultPosition();
			if (SiblingRepositionRequired)
				RepositionFormerSiblings();
		}

		protected override void OnAfterSave()
		{
			_positionWas = _position;
			_positionChanged = false;
			base.OnAfterSave();
		}

		protected override void OnAfterDestroy()
		{
			MoveLowerSiblingsUp();
			base.OnAfterDestroy();
		}

		private void MoveLowerSiblingsUp()
		{
			foreach (var sibling in LowerSiblings)
				sibling.Inc(s => s.Position, -1);
		}

		private bool SiblingRepositionRequired
		{
			get { return AncestryChanged && IsPersisted; }
		}

		private void RepositionFormerSiblings()
		{
			var formerSiblings = Find(d => d.Ancestry == AncestryWas)
				.Where(d => d.Position > _positionWas)
				.Where(d => d.ID != ID);
			foreach (var sibling in formerSiblings)
				sibling.Inc(s => s.Position, -1);
		}

		private void AssignDefaultPosition()
		{
			if (_positionChanged && !AncestryChanged)
				return;

			if (!Siblings.Any())
				Position = 0;
			else
				Position = Siblings.ToList().Max(s => s.Position) + 1;
		}

		#endregion
	}
}