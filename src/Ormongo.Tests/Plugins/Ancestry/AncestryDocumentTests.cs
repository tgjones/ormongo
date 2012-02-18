using System.Linq;
using NUnit.Framework;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	[TestFixture]
	public class AncestryDocumentTests
	{
		private class TreeNode : AncestryDocument<TreeNode>
		{
			public string Name { get; set; }
		}

		#region Ancestors

		[Test]
		public void CanGetAncestorIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act.
			var result = grandChildNode.AncestorIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
		}

		#endregion

		#region Parent

		[Test]
		public void CanGetAndSetParent()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Name = "Child"
			});

			// Act.
			childNode.Parent = rootNode;

			// Assert.
			Assert.That(childNode.Parent.ID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetAndSetParentID()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Name = "Child"
			});

			// Act.
			childNode.ParentID = rootNode.ID;

			// Assert.
			Assert.That(childNode.Parent.ID, Is.EqualTo(rootNode.ID));
		}

		#endregion

		#region Root

		[Test]
		public void CanGetRootIDForRootItem()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});

			// Act.
			var rootID = rootNode.RootID;

			// Assert.
			Assert.That(rootID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetRootIDForNonRootItem()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});

			// Act.
			var rootID = childNode.RootID;

			// Assert.
			Assert.That(rootID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetRootItem()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});

			// Act.
			var root = childNode.Root;

			// Assert.
			Assert.That(root.ID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetIsRootForRootItem()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});

			// Act / Assert.
			Assert.That(rootNode.IsRoot, Is.True);
		}

		[Test]
		public void CanGetIsRootForNonRootItem()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.IsRoot, Is.False);
		}

		#endregion

		#region Children

		[Test]
		public void CanGetChildren()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child2"
			});

			// Act.
			var children = rootNode.Children.ToList();

			// Assert.
			Assert.That(children, Has.Count.EqualTo(2));
			Assert.That(children[0].ID, Is.EqualTo(childNode1.ID));
			Assert.That(children[1].ID, Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void CanGetChildIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child2"
			});

			// Act.
			var childIDs = rootNode.ChildIDs.ToList();

			// Assert.
			Assert.That(childIDs, Has.Count.EqualTo(2));
			Assert.That(childIDs[0], Is.EqualTo(childNode1.ID));
			Assert.That(childIDs[1], Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void HasChildrenReturnsTrueForRootNodeWithChildren()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});

			// Act / Assert.
			Assert.That(rootNode.HasChildren, Is.True);
			Assert.That(rootNode.IsChildless, Is.False);
		}

		[Test]
		public void HasChildrenReturnsFalseForRootNodeWithoutChildren()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});

			// Act / Assert.
			Assert.That(rootNode.HasChildren, Is.False);
			Assert.That(rootNode.IsChildless, Is.True);
		}

		[Test]
		public void HasChildrenReturnsTrueForNonRootNodeWithChildren()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act / Assert.
			Assert.That(childNode.HasChildren, Is.True);
			Assert.That(childNode.IsChildless, Is.False);
		}

		[Test]
		public void HasChildrenReturnsFalseForNonRootNodeWithoutChildren()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.HasChildren, Is.False);
			Assert.That(childNode.IsChildless, Is.True);
		}

		#endregion

		#region Siblings

		[Test]
		public void CanGetSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child3"
			});

			// Act.
			var siblings = childNode1.Siblings.ToList();

			// Assert.
			Assert.That(siblings, Has.Count.EqualTo(3));
			Assert.That(siblings[0].ID, Is.EqualTo(childNode1.ID));
			Assert.That(siblings[1].ID, Is.EqualTo(childNode2.ID));
			Assert.That(siblings[2].ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetSiblingIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child2"
			});
			var childNode3 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child3"
			});

			// Act.
			var siblingIDs = childNode1.SiblingIDs.ToList();

			// Assert.
			Assert.That(siblingIDs, Has.Count.EqualTo(3));
			Assert.That(siblingIDs[0], Is.EqualTo(childNode1.ID));
			Assert.That(siblingIDs[1], Is.EqualTo(childNode2.ID));
			Assert.That(siblingIDs[2], Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void HasSiblingsReturnsTrueWhenNodeHasSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode1 = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child1"
			});
			TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child2"
			});

			// Act / Assert.
			Assert.That(childNode1.HasSiblings, Is.True);
			Assert.That(childNode1.IsOnlyChild, Is.False);
		}

		[Test]
		public void HasSiblingsReturnsTrueWhenNodeDoesNotHaveSiblings()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.HasSiblings, Is.False);
			Assert.That(childNode.IsOnlyChild, Is.True);
		}

		#endregion

		#region Descendants

		[Test]
		public void CanGetDescendants()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Descendants.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetDescendantIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.DescendantIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(childNode.ID));
			Assert.That(result[1], Is.EqualTo(grandChildNode.ID));
		}

		#endregion

		#region Descendants

		[Test]
		public void CanGetSubtree()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Subtree.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[2].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetSubtreeIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Parent = childNode,
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.SubtreeIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
			Assert.That(result[2], Is.EqualTo(grandChildNode.ID));
		}

		#endregion
	}
}