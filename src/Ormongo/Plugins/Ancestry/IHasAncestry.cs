namespace Ormongo.Plugins.Ancestry
{
	public interface IHasAncestry
	{
		DataDictionary TransientData { get; }
		DataDictionary ExtraData { get; }

		IAncestryProxy AncestryProxy { get; }
	}
}