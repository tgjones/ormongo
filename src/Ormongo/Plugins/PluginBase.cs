namespace Ormongo.Plugins
{
	public abstract class PluginBase : IPlugin
	{
		public virtual void Initialize()
		{
			
		}

		public virtual void BeforeSave(object document)
		{

		}

		public virtual void AfterSave(object document)
		{

		}

		public virtual void BeforeDestroy(object document)
		{
			
		}

		public virtual void AfterDestroy(object document)
		{

		}
	}
}