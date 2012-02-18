namespace Ormongo.Plugins.Ancestry
{
	public class AncestryPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			//UpdateDescendantsWithNewAncestry(document);
			base.BeforeSave(document);
		}
	}
}