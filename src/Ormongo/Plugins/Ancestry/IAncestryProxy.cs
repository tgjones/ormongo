namespace Ormongo.Plugins.Ancestry
{
	internal interface IAncestryProxy
	{
		void UpdateDescendantsWithNewAncestry();
		void ResetChangedFields();
		void ApplyOrphanStrategy();
	}
}