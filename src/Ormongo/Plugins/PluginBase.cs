using System;

namespace Ormongo.Plugins
{
	public abstract class PluginBase : IPlugin
	{
		public virtual void Initialize(Type documentType)
		{
			
		}

		public virtual void AfterFind(object document)
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