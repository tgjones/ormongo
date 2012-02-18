using NUnit.Framework;

namespace Ormongo.Tests.Plugins.Ancestry
{
	public abstract class AncestryTestsBase : TestsBase
	{
		[TearDown]
		public void TearDown()
		{
			TreeNode.Drop();
		}
	}
}