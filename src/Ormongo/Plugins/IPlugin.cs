using System;

namespace Ormongo.Plugins
{
	public interface IPlugin
	{
		void Initialize(Type documentType);

		void BeforeCreate(object document);
		void AfterCreate(object document);

		void BeforeUpdate(object document);
		void AfterUpdate(object document);

		void BeforeSave(object document);
		void AfterSave(object document);

		void BeforeDestroy(object document);
		void AfterDestroy(object document);

		void AfterFind(object document);
	}
}