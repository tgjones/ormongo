namespace Ormongo.Plugins.Ancestry
{
	public interface IAncestryProxy
	{
		void UpdateDescendantsWithNewAncestry();
		void ResetChangedFields();
	}
}