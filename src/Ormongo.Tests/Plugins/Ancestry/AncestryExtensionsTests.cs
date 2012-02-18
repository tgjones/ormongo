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
			var rootNode1 = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode1 },
				Name = "Child"
			});
			TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});
			var rootNode2 = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});

			// Act.
			var result = TreeNode.FindAll().Roots().ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode1.ID));
			Assert.That(result[1].ID, Is.EqualTo(rootNode2.ID));
		}
	}
}