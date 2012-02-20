using NUnit.Framework;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	public abstract class AncestryTestsBase : TestsBase
	{
		[SetUp]
		public void SetUp()
		{
			TreeNode.OrphanStrategy = OrphanStrategy.Destroy;
		}

		[TearDown]
		public void TearDown()
		{
			TreeNode.Drop();
		}

		protected static TreeNode CreateTreeNode(TreeNode parent, string name)
		{
			return TreeNode.Create(new TreeNode
			{
				Parent = parent,
				Name = name
			});
		}
	}
}