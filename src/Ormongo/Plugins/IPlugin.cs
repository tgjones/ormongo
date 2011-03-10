using MongoDB.Bson.DefaultSerializer;

namespace Ormongo.Plugins
{
	public interface IPlugin
	{
		void Initialize();
	}
}