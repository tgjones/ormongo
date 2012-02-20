using System.Linq;
using NUnit.Framework;

namespace Ormongo.Tests.Plugins.Ancestry
{
	[TestFixture]
	public class OrderingProxyTests : AncestryTestsBase
	{
		[Test]
		public void CanGetLowerSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 2 },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 1 },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 3 },
				Name = "Child3"
			});

			// Act.
			var result = childNode1.Ordering.LowerSiblings.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetHigherSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 2 },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 1 },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 3 },
				Name = "Child3"
			});

			// Act.
			var result = childNode1.Ordering.HigherSiblings.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(childNode2.ID));
		}
	}
}