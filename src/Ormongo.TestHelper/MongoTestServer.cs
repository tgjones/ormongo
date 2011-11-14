using System.Diagnostics;
using System.IO;

namespace Ormongo.TestHelper
{
	public class MongoTestServer
	{
		private readonly string _mongodPath;
		private readonly string _databaseName;
		private readonly int _port;
		private Process _process;

		public MongoTestServer(string mongodPath, string databaseName, int port = 28000)
		{
			_mongodPath = mongodPath;
			_databaseName = databaseName;
			_port = port;
		}

		public void Start()
		{
			OrmongoConfiguration.ServerHost = "localhost";
			OrmongoConfiguration.ServerPort = _port;
			OrmongoConfiguration.Database = _databaseName;

			Directory.CreateDirectory("data");

			_process = Process.Start(new ProcessStartInfo(_mongodPath, "--dbpath \"data\" --port " + _port));
		}

		public void Stop()
		{
			_process.CloseMainWindow();
			_process.WaitForExit(5000);
			if (!_process.HasExited)
				_process.Kill();

			Directory.Delete("data", true);
		}
	}
}
