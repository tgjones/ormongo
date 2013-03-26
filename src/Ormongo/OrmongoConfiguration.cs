using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Ormongo.Internal.Serialization;

namespace Ormongo
{
	public static class OrmongoConfiguration
	{
		private static bool _initialized;

		public static string ServerHost { get; set; }
		public static int ServerPort { get; set; }
		public static string Database { get; set; }
		public static bool SafeMode { get; set; }
		public static TimeSpan ConnectTimeout { get; set; }
		public static TimeSpan SocketTimeout { get; set; }
		public static TimeSpan WaitQueueTimeout { get; set; }

		public static bool AutoCreateIndexes { get; set; }

		static OrmongoConfiguration()
		{
			SafeMode = true;
			ConnectTimeout = MongoDefaults.ConnectTimeout;
			SocketTimeout = MongoDefaults.SocketTimeout;
			WaitQueueTimeout = MongoDefaults.WaitQueueTimeout;
		}

		internal static void Initialize()
		{
			if (_initialized)
				return;

			// Register custom serialization provider.
			BsonSerializer.RegisterSerializationProvider(new SerializationProvider());

			_initialized = true;
		}

		internal static MongoServer GetMongoServer()
		{
			var serverAddress = (ServerPort != 0)
				? new MongoServerAddress(ServerHost, ServerPort)
				: new MongoServerAddress(ServerHost);
			var connectionStringBuilder = new MongoConnectionStringBuilder
			{
				Server = serverAddress,
				ConnectTimeout = ConnectTimeout,
				SocketTimeout = SocketTimeout,
				WaitQueueTimeout = WaitQueueTimeout,
				SafeMode = (SafeMode)
					? MongoDB.Driver.SafeMode.True
					: MongoDB.Driver.SafeMode.False
			};
			return MongoServer
				.Create(connectionStringBuilder.ToServerSettings());
		}

		internal static MongoDatabase GetMongoDatabase()
		{
			return GetMongoServer().GetDatabase(Database);
		}

		public static void CloseConnection()
		{
			GetMongoServer().Disconnect();
		}

		public static void DropAllCollections()
		{
			var database = GetMongoDatabase();
			foreach (var collectionName in GetCollectionNames(database))
				database.DropCollection(collectionName);
		}

		private static IEnumerable<string> GetCollectionNames(MongoDatabase database)
		{
			return database.GetCollectionNames().Where(s => !s.StartsWith("system."));
		}

		public static DatabaseStatistics GetDatabaseStatistics()
		{
			var database = GetMongoDatabase();
			var stats = database.GetStats();
			return new DatabaseStatistics
			{
				AverageObjectSize = stats.AverageObjectSize,
				CollectionCount = stats.CollectionCount,
				DataSize = stats.DataSize,
				ExtentCount = stats.ExtentCount,
				FileSize = stats.FileSize,
				IndexCount = stats.IndexCount,
				IndexSize = stats.IndexSize,
				ObjectCount = stats.ObjectCount,
				StorageSize = stats.StorageSize,
				CollectionStatistics = GetCollectionNames(database).Select(s => new CollectionStatistics
				{
					CollectionName = s,
					Count = database.GetCollection(s).Count()
				})
			};
		}
	}

	public class CollectionStatistics
	{
		public string CollectionName { get; set; }
		public long Count { get; set; }
	}
}