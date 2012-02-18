using Ormongo.Internal;

namespace Ormongo.Plugins
{
	public class AssociationPlugin : PluginBase
	{
		public override void AfterFind(object document)
		{
			AssociationUtility.UpdateAssociations(document);
			base.AfterFind(document);
		}

		public override void BeforeSave(object document)
		{
			AssociationUtility.UpdateAssociations(document);
			base.BeforeSave(document);
		}
	}
}