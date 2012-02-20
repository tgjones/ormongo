namespace Ormongo.Plugins.Ancestry
{
	internal interface IAncestryProxy
	{
		void SetWasValues();
		void UpdateDescendantsWithNewAncestry();
		void ResetChangedFields();
		void ApplyOrphanStrategy(OrphanStrategy orphanStrategy);
	}
}