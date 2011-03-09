using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[SetUpFixture]
	public class MongoServerSetup
	{
		private Process _process;

		[SetUp]
		public void SetUp()
		{
			OrmongoConfiguration.ConnectionString = "mongodb://localhost:28000";
			OrmongoConfiguration.Database = "OrmongoTests";

			Directory.CreateDirectory("data");

			_process = Process.Start(new ProcessStartInfo(@"..\..\..\..\tools\mongodb\bin\mongod",
				"--dbpath \"data\" --port 28000"));
		}

		[TearDown]
		public void TearDown()
		{
			_process.CloseMainWindow();
			_process.WaitForExit(5000);
			if (!_process.HasExited)
				_process.Kill();

			Directory.Delete("data", true);
		}
	}
}