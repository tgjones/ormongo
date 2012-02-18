using System.Collections.Generic;
using System.Diagnostics;

namespace Ormongo.Tests
{
	public abstract class TestsBase
	{
		public void Log(Dictionary<string, string> info)
		{
			foreach (var item in info)
				Trace.WriteLine(string.Format("{0} : {1}", item.Key, item.Value));
		}
	}
}