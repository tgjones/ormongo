using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public static class AncestryExtensions
	{
		public static IQueryable<T> Roots<T>(this IQueryable<T> items)
			where T : Document<T>, IHasAncestry<T>
		{
			return items.Where(d => d.ExtraData[AncestryProxy<T>.AncestryKey] == null);
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
	}
}