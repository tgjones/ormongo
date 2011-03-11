namespace Ormongo.Plugins
{
	public interface IPlugin
	{
		void Initialize();
		void BeforeSave(object document);
	}
}