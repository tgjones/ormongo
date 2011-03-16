using Ormongo.Internal;

namespace Ormongo.Plugins
{
	public class AssociationPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			AssociationUtility.UpdateAssociations(document);
			base.BeforeSave(document);
		}
	}
}