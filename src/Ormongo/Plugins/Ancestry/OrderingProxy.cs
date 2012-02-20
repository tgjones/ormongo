using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public class OrderingProxy<T>
		where T : Document<T>
	{
		internal const string PositionKey = "Position";

		static OrderingProxy()
		{
			Document<T>.DefaultScope = items => items.OrderBy(d => d.ExtraData[PositionKey]);
		}

		private readonly T _instance;
		private readonly AncestryProxy<T> _ancestry;

		public OrderingProxy(T instance)
		{
			_instance = instance;
			_ancestry = new AncestryProxy<T>(instance);
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
	}
}