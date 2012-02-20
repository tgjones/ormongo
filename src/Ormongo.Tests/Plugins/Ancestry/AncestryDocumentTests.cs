using System.Linq;
using NUnit.Framework;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	[TestFixture]
	public class AncestryDocumentTests : AncestryTestsBase
	{
		#region Instance

		#region Ancestors

		[Test]
		public void CanGetAncestorIDs()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");


			// Act.
			var result = grandChildNode.AncestorIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
		}

		[Test]
		public void CanGetAncestors()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = grandChildNode.Ancestors.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
		}

		[Test]
		public void CanGetAncestorsAndSelfIDs()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = grandChildNode.AncestorsAndSelfIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
			Assert.That(result[2], Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetAncestorsAndSelf()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = grandChildNode.AncestorsAndSelf.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[2].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetDepth()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act / Assert.
			Assert.That(grandChildNode.Depth, Is.EqualTo(2));
		}

		#endregion

		#region Parent

		[Test]
		public void CanGetAndSetParent()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act.
			childNode.Parent = rootNode;

			// Assert.
			Assert.That(childNode.Parent.ID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetAndSetParentID()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

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
			var rootNode = CreateTreeNode(null, "Root");

			// Act.
			var rootID = rootNode.RootID;

			// Assert.
			Assert.That(rootID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetRootIDForNonRootItem()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act.
			var rootID = childNode.RootID;

			// Assert.
			Assert.That(rootID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetRootItem()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act.
			var root = childNode.Root;

			// Assert.
			Assert.That(root.ID, Is.EqualTo(rootNode.ID));
		}

		[Test]
		public void CanGetIsRootForRootItem()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");

			// Act / Assert.
			Assert.That(rootNode.IsRoot, Is.True);
		}

		[Test]
		public void CanGetIsRootForNonRootItem()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act / Assert.
			Assert.That(childNode.IsRoot, Is.False);
		}

		#endregion

		#region Children

		[Test]
		public void CanGetChildren()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");

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
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");

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
			var rootNode = CreateTreeNode(null, "Root");
			CreateTreeNode(rootNode, "Child");

			// Act / Assert.
			Assert.That(rootNode.HasChildren, Is.True);
			Assert.That(rootNode.IsChildless, Is.False);
		}

		[Test]
		public void HasChildrenReturnsFalseForRootNodeWithoutChildren()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");

			// Act / Assert.
			Assert.That(rootNode.HasChildren, Is.False);
			Assert.That(rootNode.IsChildless, Is.True);
		}

		[Test]
		public void HasChildrenReturnsTrueForNonRootNodeWithChildren()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			CreateTreeNode(childNode, "GrandChild");

			// Act / Assert.
			Assert.That(childNode.HasChildren, Is.True);
			Assert.That(childNode.IsChildless, Is.False);
		}

		[Test]
		public void HasChildrenReturnsFalseForNonRootNodeWithoutChildren()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act / Assert.
			Assert.That(childNode.HasChildren, Is.False);
			Assert.That(childNode.IsChildless, Is.True);
		}

		[Test]
		public void CanCreateChildThroughChildrenProperty()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
		
			// Act.
			var childNode = rootNode.Children.Create(new TreeNode
			{
				Name = "Child"
			});

			// Assert.
			Assert.That(childNode.Parent.ID, Is.EqualTo(rootNode.ID));
		}

		#endregion

		#region Siblings

		[Test]
		public void CanGetSiblingsAndSelf()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			var siblings = childNode1.SiblingsAndSelf.ToList();

			// Assert.
			Assert.That(siblings, Has.Count.EqualTo(3));
			Assert.That(siblings[0].ID, Is.EqualTo(childNode1.ID));
			Assert.That(siblings[1].ID, Is.EqualTo(childNode2.ID));
			Assert.That(siblings[2].ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			var siblings = childNode1.Siblings.ToList();

			// Assert.
			Assert.That(siblings, Has.Count.EqualTo(2));
			Assert.That(siblings[0].ID, Is.EqualTo(childNode2.ID));
			Assert.That(siblings[1].ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetSiblingIDs()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			var siblingIDs = childNode1.SiblingIDs.ToList();

			// Assert.
			Assert.That(siblingIDs, Has.Count.EqualTo(2));
			Assert.That(siblingIDs[0], Is.EqualTo(childNode2.ID));
			Assert.That(siblingIDs[1], Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void HasSiblingsReturnsTrueWhenNodeHasSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			CreateTreeNode(rootNode, "Child2");

			// Act / Assert.
			Assert.That(childNode1.HasSiblings, Is.True);
			Assert.That(childNode1.IsOnlyChild, Is.False);
		}

		[Test]
		public void HasSiblingsReturnsTrueWhenNodeDoesNotHaveSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");

			// Act / Assert.
			Assert.That(childNode.HasSiblings, Is.False);
			Assert.That(childNode.IsOnlyChild, Is.True);
		}

		#endregion

		#region Descendants

		[Test]
		public void CanGetDescendantsAndSelf()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = rootNode.DescendantsAndSelf.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[2].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetDescendantsAndSelfIDs()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = rootNode.DescendantsAndSelfIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
			Assert.That(result[2], Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetDescendants()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

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
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = rootNode.DescendantIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(childNode.ID));
			Assert.That(result[1], Is.EqualTo(grandChildNode.ID));
		}

		#endregion

		#region Callbacks

		[Test]
		public void CanMoveNodeWithinTree()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var grandChildNode = CreateTreeNode(childNode1, "GrandChild");
			var greatGrandChildNode = CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			grandChildNode.Parent = childNode2;
			grandChildNode.Save();

			// Assert.
			greatGrandChildNode = TreeNode.FindOneByID(greatGrandChildNode.ID);
			Assert.That(greatGrandChildNode.AncestorIDs, Contains.Item(childNode2.ID));
		}

		[Test]
		public void CanUseDestroyOrphanStrategy()
		{
			// Arrange.
			TreeNode.OrphanStrategy = OrphanStrategy.Destroy;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			childNode.Destroy();

			// Assert.
			Assert.That(TreeNode.FindOneByID(childNode.ID), Is.Null);
			Assert.That(TreeNode.FindOneByID(grandChildNode.ID), Is.Null);
		}

		[Test, ExpectedException]
		public void CanUseRestrictOrphanStrategy()
		{
			// Arrange.
			TreeNode.OrphanStrategy = OrphanStrategy.Restrict;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			CreateTreeNode(childNode, "GrandChild");

			childNode.Destroy();
		}
		
		[Test]
		public void CanUseRootifyOrphanStrategy()
		{
			// Arrange.
			TreeNode.OrphanStrategy = OrphanStrategy.Rootify;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			childNode.Destroy();

			// Assert.
			Assert.That(TreeNode.FindOneByID(childNode.ID), Is.Null);
			Assert.That(TreeNode.FindOneByID(grandChildNode.ID).Parent, Is.Null);
		}

		#endregion

		#region Inheritance

		[Test]
		public void InheritedClassesAreStoredInTheSameCollection()
		{
			// Act.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var folderNode = TreeNode.Create(new FolderNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			TreeNode.Create(new FileNode
			{
				Parent = folderNode,
				Name = "GrandChild"
			});

			// Assert.
			Assert.That(TreeNode.FindAll().ToList(), Has.Count.EqualTo(3));
		}

		[Test]
		public void AncestryProxyOperatesOnWholeInheritanceTree()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var folderNode = TreeNode.Create(new FolderNode
			{
				Parent = rootNode,
				Name = "Child"
			});
			var fileNode = TreeNode.Create(new FileNode
			{
				Parent = folderNode,
				Name = "GrandChild"
			});

			// Act.
			var root = fileNode.Root;

			// Assert.
			Assert.That(root.ID, Is.EqualTo(rootNode.ID));
		}

		#endregion

		#endregion
	}
}