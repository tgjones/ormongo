using System.ComponentModel.DataAnnotations;

namespace Ormongo.Plugins
{
	public class ValidationPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			var context = new ValidationContext(document, null, null);
			Validator.ValidateObject(document, context);
		}
	}
}