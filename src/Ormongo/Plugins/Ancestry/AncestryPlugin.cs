namespace Ormongo.Plugins.Ancestry
{
	public class AncestryPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			if (HasAncestry(document))
				GetAncestryProxy(document).UpdateDescendantsWithNewAncestry();
			base.BeforeSave(document);
		}

		public override void AfterSave(object document)
		{
			if (HasAncestry(document))
				GetAncestryProxy(document).ResetChangedFields();
			base.AfterSave(document);
		}

		public override void BeforeDestroy(object document)
		{
			if (HasAncestry(document))
				GetAncestryProxy(document).ApplyOrphanStrategy();
			base.BeforeDestroy(document);
		}

		private static bool HasAncestry(object document)
		{
			return document.GetType().GetInterface("IHasAncestry`1") != null;
		}

		private static IAncestryProxy GetAncestryProxy(object document)
		{
			return (IAncestryProxy) document.GetType().GetProperty("Ancestry").GetValue(document, null);
		}
	}
}