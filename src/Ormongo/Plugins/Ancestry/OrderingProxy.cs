using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public class OrderingProxy<T>
		where T : Document<T>
	{
		internal const string PositionKey = "Position";

		private readonly T _instance;
		private readonly AncestryProxy<T> _ancestryProxy;

		public OrderingProxy(T instance)
		{
			_instance = instance;
			_ancestryProxy = new AncestryProxy<T>(instance);
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
			get { return _ancestryProxy.Siblings.Where(d => d.ExtraData[PositionKey] > Position); }
		}

		/// <summary>
		/// Returns siblings above the current document - i.e. siblings with
		/// a position less than than this document's position.
		/// </summary>
		public IQueryable<T> HigherSiblings
		{
			get { return _ancestryProxy.Siblings.Where(d => d.ExtraData[PositionKey] < Position); }
		}
	}
}