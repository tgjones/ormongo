using System;
using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public class OrderingProxy<T> : IOrderingProxy
		where T : Document<T>
	{
		internal const string PositionKey = "Position";

		private readonly T _instance;
		private readonly AncestryProxy<T> _ancestry;

		#region Static

		static OrderingProxy()
		{
			Document<T>.DefaultScope = items => items.OrderBy(d => d.ExtraData[PositionKey]);
		}

		internal static OrderingProxy<T> ProxyFor(T instance)
		{
			return new OrderingProxy<T>(instance);
		}

		#endregion

		public OrderingProxy(T instance)
		{
			_instance = instance;
			_ancestry = new AncestryProxy<T>(instance);
		}

		private bool HasPosition
		{
			get { return _instance.ExtraData.Contains(PositionKey); }
		}

		public int Position
		{
			get { return _instance.ExtraData.SafeGet<int>(PositionKey); }
			set { _instance.ExtraData.SafeSet(PositionKey, value); }
		}

		/// <summary>
		/// Returns siblings below the current document - i.e. siblings with
		/// a position greater than this document's position.
		/// </summary>
		public IQueryable<T> LowerSiblings
		{
			get { return _ancestry.Siblings.Where(d => d.ExtraData[PositionKey] > Position); }
		}

		/// <summary>
		/// Returns siblings above the current document - i.e. siblings with
		/// a position less than than this document's position.
		/// </summary>
		public IQueryable<T> HigherSiblings
		{
			get { return _ancestry.Siblings.Where(d => d.ExtraData[PositionKey] < Position); }
		}

		/// <summary>
		/// Gets the lowest sibling (could be this)
		/// </summary>
		public T LowestSibling
		{
			get { return _ancestry.SiblingsAndSelf.ToList().Last(); }
		}

		/// <summary>
		/// Gets the highest sibling (could be this)
		/// </summary>
		public T HighestSibling
		{
			get { return _ancestry.SiblingsAndSelf.First(); }
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
			if (!_ancestry.IsSiblingOf(other))
			{
				_ancestry.ParentID = AncestryProxy<T>.ProxyFor(other).ParentID;
				_instance.Save();
			}

			var otherProxy = ProxyFor(other);
			if (Position > otherProxy.Position)
			{
				int newPosition = otherProxy.Position;
				foreach (var sibling in otherProxy.LowerSiblings.Where(d => d.ExtraData[PositionKey] < Position))
					sibling.Inc(s => s.ExtraData[PositionKey], 1);
				other.Inc(s => s.ExtraData[PositionKey], 1);
				Position = newPosition;
				_instance.Save();
			}
			else
			{
				int newPosition = otherProxy.Position - 1;
				foreach (var sibling in otherProxy.HigherSiblings.Where(d => d.ExtraData[PositionKey] > Position))
					sibling.Inc(s => s.ExtraData[PositionKey], -1);
				Position = newPosition;
				_instance.Save();
			}
		}

		/// <summary>
		/// Moves this node below the specified node.
		/// Changes the node's parent if necessary.
		/// </summary>
		/// <param name="other"></param>
		public void MoveBelow(T other)
		{
			if (!_ancestry.IsSiblingOf(other))
			{
				_ancestry.ParentID = AncestryProxy<T>.ProxyFor(other).ParentID;
				_instance.Save();
			}

			var otherProxy = ProxyFor(other);
			if (Position > otherProxy.Position)
			{
				int newPosition = otherProxy.Position + 1;
				foreach (var sibling in otherProxy.LowerSiblings.Where(d => d.ExtraData[PositionKey] < Position))
					sibling.Inc(s => s.ExtraData[PositionKey], 1);
				Position = newPosition;
				_instance.Save();
			}
			else
			{
				int newPosition = otherProxy.Position;
				foreach (var sibling in otherProxy.HigherSiblings.Where(d => d.ExtraData[PositionKey] > Position))
					sibling.Inc(s => s.ExtraData[PositionKey], -1);
				other.Inc(s => s.ExtraData[PositionKey], 1);
				Position = newPosition;
				_instance.Save();
			}
		}

		#region Helper methods

		private void MoveLowerSiblingsUp()
		{
			foreach (var sibling in LowerSiblings)
				sibling.Inc(s => s.ExtraData[PositionKey], -1);
		}

		private void RepositionFormerSiblings()
		{
			throw new System.NotImplementedException();
		}

		void IOrderingProxy.AssignDefaultPosition()
		{
			if (HasPosition && !_ancestry.AncestryChanged)
				return;

			if (!_ancestry.Siblings.Any() || !_ancestry.Siblings.Select(s => s.ExtraData[PositionKey]).Select(p => p != null).Any())
				Position = 0;
			else
				Position = _ancestry.Siblings.Max(s => (int) s.ExtraData[PositionKey]) + 1;
		}

		#endregion

		/*
		 *  private

      def move_lower_siblings_up
        lower_siblings.each { |s| s.inc(:position, -1) }
      end

      def reposition_former_siblings
        former_siblings = base_class.where(:parent_id => attribute_was('parent_id')).
                                     and(:position.gt => (attribute_was('position') || 0)).
                                     excludes(:id => self.id)
        former_siblings.each { |s| s.inc(:position,  -1) }
      end

      def sibling_reposition_required?
        parent_id_changed? && persisted?
      end

      def assign_default_position
        return unless self.position.nil? || self.parent_id_changed?

        if self.siblings.empty? || self.siblings.collect(&:position).compact.empty?
          self.position = 0
        else
          self.position = self.siblings.max(:position).to_i + 1
        end
      end
    end
		 * */
	}
}