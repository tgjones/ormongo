using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Ormongo.Tests
{
	public abstract class TestsBase
	{
		[TearDown]
		public virtual void TearDown()
		{
			BlogPost.Drop();
			Book.Drop();
		}

		public void Log(Dictionary<string, string> info)
		{
			foreach (var item in info)
				Trace.WriteLine(string.Format("{0} : {1}", item.Key, item.Value));
		}
	}
}