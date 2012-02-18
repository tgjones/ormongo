namespace Ormongo.Plugins.Ancestry
{
	public class AncestryPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			var hasAncestryDocument = document as IHasAncestry;
			if (hasAncestryDocument != null)
				hasAncestryDocument.AncestryProxy.UpdateDescendantsWithNewAncestry();
			base.BeforeSave(document);
		}

		public override void AfterSave(object document)
		{
			var hasAncestryDocument = document as IHasAncestry;
			if (hasAncestryDocument != null)
				hasAncestryDocument.AncestryProxy.ResetChangedFields();
			base.AfterSave(document);
		}
	}
}