using MongoDB.Driver;

namespace Ormongo
{
	public static class OrmongoConfiguration
	{
		public static string ConnectionString { get; set; }
		public static string Database { get; set; }

		public static bool AutoCreateIndexes { get; set; }

		internal static MongoDatabase GetMongoDatabase()
		{
			return MongoServer.Create(ConnectionString).GetDatabase(Database);
		}
	}
}