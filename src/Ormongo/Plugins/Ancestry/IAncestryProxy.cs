namespace Ormongo.Plugins.Ancestry
{
	internal interface IAncestryProxy
	{
		void UpdateDescendantsWithNewAncestry();
		void ApplyOrphanStrategy(OrphanStrategy orphanStrategy);
	}
}