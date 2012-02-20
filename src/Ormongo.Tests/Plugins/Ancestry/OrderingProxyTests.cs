using System.Linq;
using FluentMongo.Linq;
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

		[Test]
		public void CanGetLowestSibling()
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
			var result = childNode1.Ordering.LowestSibling;

			// Assert.
			Assert.That(result.ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetHighestSibling()
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
			var result = childNode1.Ordering.HighestSibling;

			// Assert.
			Assert.That(result.ID, Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void CanMoveToTop()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 1 },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 2 },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Ordering = { Position = 3 },
				Name = "Child3"
			});

			// Act.
			childNode3.Ordering.MoveToTop();

			// Assert.
			var children = rootNode.Ancestry.Children.QueryDump(Log).ToList();
			Assert.That(children[0].ID, Is.EqualTo(childNode3.ID));
			Assert.That(children[1].ID, Is.EqualTo(childNode1.ID));
			Assert.That(children[2].ID, Is.EqualTo(childNode2.ID));
		}
	}
}