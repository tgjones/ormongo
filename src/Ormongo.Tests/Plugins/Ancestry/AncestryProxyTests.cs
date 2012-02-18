using System.Linq;
using FluentMongo.Linq;
using NUnit.Framework;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	[TestFixture]
	public class AncestryProxyTests : TestsBase
	{
		[TearDown]
		public void TearDown()
		{
			TreeNode.Drop();
		}

		private class TreeNode : Document<TreeNode>, IHasAncestry
		{
			public string Name { get; set; }

			private AncestryProxy<TreeNode> _ancestry;
			public AncestryProxy<TreeNode> Ancestry
			{
				get { return _ancestry ?? (_ancestry = new AncestryProxy<TreeNode>(this)); }
			}

			IAncestryProxy IHasAncestry.AncestryProxy
			{
				get { return Ancestry; }
			}
		}

		#region Static

		#endregion

		#region Instance

		[Test]
		public void CanMoveNodeWithinTree()
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
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode1 },
				Name = "GrandChild"
			});
			var greatGrandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = grandChildNode },
				Name = "GreatGrandChild"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});

			// Act.
			grandChildNode.Ancestry.Parent = childNode2;
			grandChildNode.Save();

			// Assert.
			greatGrandChildNode = TreeNode.FindOneByID(greatGrandChildNode.ID);
			Assert.That(greatGrandChildNode.Ancestry.AncestorIDs, Contains.Item(childNode2.ID));
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = grandChildNode.Ancestry.AncestorIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
		}

		[Test]
		public void CanGetAncestors()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = grandChildNode.Ancestry.Ancestors.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
		}

		[Test]
		public void CanGetPathIDs()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = grandChildNode.Ancestry.PathIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
			Assert.That(result[2], Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void CanGetPath()
		{
			// Arrange.
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = grandChildNode.Ancestry.Path.ToList();

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
			var rootNode = TreeNode.Create(new TreeNode
			{
				Name = "Root"
			});
			var childNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			Assert.That(grandChildNode.Ancestry.Depth, Is.EqualTo(2));
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
			childNode.Ancestry.Parent = rootNode;

			// Assert.
			Assert.That(childNode.Ancestry.Parent.ID, Is.EqualTo(rootNode.ID));
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
			childNode.Ancestry.ParentID = rootNode.ID;

			// Assert.
			Assert.That(childNode.Ancestry.Parent.ID, Is.EqualTo(rootNode.ID));
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
			var rootID = rootNode.Ancestry.RootID;

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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Act.
			var rootID = childNode.Ancestry.RootID;

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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Act.
			var root = childNode.Ancestry.Root;

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
			Assert.That(rootNode.Ancestry.IsRoot, Is.True);
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.Ancestry.IsRoot, Is.False);
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
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});

			// Act.
			var children = rootNode.Ancestry.Children.ToList();

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
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});
			var childNode2 = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});

			// Act.
			var childIDs = rootNode.Ancestry.ChildIDs.ToList();

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
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});

			// Act / Assert.
			Assert.That(rootNode.Ancestry.HasChildren, Is.True);
			Assert.That(rootNode.Ancestry.IsChildless, Is.False);
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
			Assert.That(rootNode.Ancestry.HasChildren, Is.False);
			Assert.That(rootNode.Ancestry.IsChildless, Is.True);
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act / Assert.
			Assert.That(childNode.Ancestry.HasChildren, Is.True);
			Assert.That(childNode.Ancestry.IsChildless, Is.False);
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.Ancestry.HasChildren, Is.False);
			Assert.That(childNode.Ancestry.IsChildless, Is.True);
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
			var siblings = childNode1.Ancestry.Siblings.ToList();

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
			var siblingIDs = childNode1.Ancestry.SiblingIDs.ToList();

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
				Ancestry = { Parent = rootNode },
				Name = "Child1"
			});
			TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = rootNode },
				Name = "Child2"
			});

			// Act / Assert.
			Assert.That(childNode1.Ancestry.HasSiblings, Is.True);
			Assert.That(childNode1.Ancestry.IsOnlyChild, Is.False);
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});

			// Act / Assert.
			Assert.That(childNode.Ancestry.HasSiblings, Is.False);
			Assert.That(childNode.Ancestry.IsOnlyChild, Is.True);
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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Ancestry.Descendants.QueryDump(Log).ToList();

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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Ancestry.DescendantIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0], Is.EqualTo(childNode.ID));
			Assert.That(result[1], Is.EqualTo(grandChildNode.ID));
		}

		#endregion

		#region Subtree

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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Ancestry.Subtree.ToList();

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
				Ancestry = { Parent = rootNode },
				Name = "Child"
			});
			var grandChildNode = TreeNode.Create(new TreeNode
			{
				Ancestry = { Parent = childNode },
				Name = "GrandChild"
			});

			// Act.
			var result = rootNode.Ancestry.SubtreeIDs.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0], Is.EqualTo(rootNode.ID));
			Assert.That(result[1], Is.EqualTo(childNode.ID));
			Assert.That(result[2], Is.EqualTo(grandChildNode.ID));
		}

		#endregion

		#endregion
	}
}