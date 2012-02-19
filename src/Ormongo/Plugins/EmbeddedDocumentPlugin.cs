using Ormongo.Internal;

namespace Ormongo.Plugins
{
	public class EmbeddedDocumentPlugin : PluginBase
	{
		public override void AfterFind(object document)
		{
			EmbeddedDocumentUtility.UpdateParentReferences(document);
			base.AfterFind(document);
		}

		public override void BeforeSave(object document)
		{
			EmbeddedDocumentUtility.UpdateParentReferences(document);
			base.BeforeSave(document);
		}
	}
}