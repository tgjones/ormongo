namespace Ormongo.Plugins.Ancestry
{
	public interface IHasAncestry<T>
		where T : Document<T>
	{
		AncestryProxy<T> Ancestry { get; }
	}
}