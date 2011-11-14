using MongoDB.Driver;

namespace Ormongo
{
	public static class OrmongoConfiguration
	{
		public static string ServerHost { get; set; }
		public static int ServerPort { get; set; }
		public static string Database { get; set; }
		public static bool SafeMode { get; set; }

		public static bool AutoCreateIndexes { get; set; }

		internal static MongoDatabase GetMongoDatabase()
		{
			var serverAddress = (ServerPort != 0)
				? new MongoServerAddress(ServerHost, ServerPort)
				: new MongoServerAddress(ServerHost);
			var connectionStringBuilder = new MongoConnectionStringBuilder
			{
				Server = serverAddress,
				SafeMode = (SafeMode)
					? MongoDB.Driver.SafeMode.True
					: MongoDB.Driver.SafeMode.False
			};
			return MongoServer
				.Create(connectionStringBuilder.ToServerSettings())
				.GetDatabase(Database);
		}

		public static void CloseConnection()
		{
			GetMongoDatabase().Server.Disconnect();
		}
	}
}