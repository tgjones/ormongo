using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	[Ancestry(OrderingEnabled = true)]
	public class TreeNode : Document<TreeNode>, IHasAncestry
	{
		public string Name { get; set; }

		private AncestryProxy<TreeNode> _ancestry;
		public AncestryProxy<TreeNode> Ancestry
		{
			get { return _ancestry ?? (_ancestry = new AncestryProxy<TreeNode>(this)); }
		}

		private OrderingProxy<TreeNode> _ordering;
		public OrderingProxy<TreeNode> Ordering
		{
			get { return _ordering ?? (_ordering = new OrderingProxy<TreeNode>(this)); }
		}
	}

	public class FolderNode : TreeNode
	{

	}

	public class FileNode : TreeNode
	{

	}
}