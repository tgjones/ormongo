using System.Linq;
using NUnit.Framework;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	public class AncestryExtensionsTests : AncestryTestsBase
	{
		[Test]
		public void CanGetRoots()
		{
			// Arrange.
			var rootNode1 = CreateTreeNode(null, "Root1");
			var childNode = CreateTreeNode(rootNode1, "Child");
			CreateTreeNode(childNode, "GrandChild");
			var rootNode2 = CreateTreeNode(null, "Root2");

			// Act.
			var result = TreeNode.FindAll().Roots().ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode1.ID));
			Assert.That(result[1].ID, Is.EqualTo(rootNode2.ID));
		}
	}
}