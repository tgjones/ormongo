namespace Ormongo.Plugins.Ancestry
{
	public interface IHasAncestry
	{
		IAncestryProxy AncestryProxy { get; }
	}
}