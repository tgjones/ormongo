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
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});
			childNode1.Ordering.Position = 2;
			childNode1.Save();
			childNode2.Ordering.Position = 1;
			childNode2.Save();
			childNode3.Ordering.Position = 3;
			childNode3.Save();

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
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});
			childNode1.Ordering.Position = 2;
			childNode1.Save();
			childNode2.Ordering.Position = 1;
			childNode2.Save();
			childNode3.Ordering.Position = 3;
			childNode3.Save();

			// Act.
			var result = childNode1.Ordering.HigherSiblings.QueryDump(Log).ToList();

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
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});
			childNode1.Ordering.Position = 3;
			childNode1.Save();
			childNode2.Ordering.Position = 1;
			childNode2.Save();
			childNode3.Ordering.Position = 2;
			childNode3.Save();

			// Act.
			var result = childNode2.Ordering.LowestSibling;

			// Assert.
			Assert.That(result.ID, Is.EqualTo(childNode1.ID));
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
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});
			childNode1.Ordering.Position = 2;
			childNode1.Save();
			childNode2.Ordering.Position = 1;
			childNode2.Save();
			childNode3.Ordering.Position = 3;
			childNode3.Save();

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
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
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

		#region Callbacks

		[Test]
		public void SetsDefaultPosition()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			
			// Act.
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});

			// Assert.
			Assert.That(childNode1.Ordering.Position, Is.EqualTo(0));
			Assert.That(childNode2.Ordering.Position, Is.EqualTo(1));
			Assert.That(childNode3.Ordering.Position, Is.EqualTo(2));
		}

		[Test]
		public void SetsDefaultPositionWithNoSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});

			// Act.
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Assert.
			Assert.That(childNode.Ordering.Position, Is.EqualTo(0));
		}

		[Test]
		public void LowerSiblingsAreMovedUpAfterDestroy()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child3"
			});

			// Act.
			childNode1.Destroy();

			// Assert.
			Assert.That(TreeNode.FindOneByID(childNode2.ID).Ordering.Position, Is.EqualTo(0));
			Assert.That(TreeNode.FindOneByID(childNode3.ID).Ordering.Position, Is.EqualTo(1));
		}

		[Test]
		public void RepositionsFormerSiblings()
		{
			// Arrange.
			var rootNode1 = TreeNode.Create(new TreeNode
			{
				Name = "Root1"
			});
			var rootNode2 = TreeNode.Create(new TreeNode
			{
				Name = "Root 2"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode1 },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode1 },
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode1 },
				Name = "Child3"
			});

			// Act.
			childNode2.Ancestry.Parent = rootNode2;
			childNode2.Save();

			// Assert.
			Assert.That(TreeNode.FindOneByID(childNode1.ID).Ordering.Position, Is.EqualTo(0));
			Assert.That(TreeNode.FindOneByID(childNode2.ID).Ordering.Position, Is.EqualTo(0));
			Assert.That(TreeNode.FindOneByID(childNode3.ID).Ordering.Position, Is.EqualTo(1));
		}

		#endregion
	}
}