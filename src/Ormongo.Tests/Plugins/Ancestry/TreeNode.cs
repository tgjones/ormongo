using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	internal class TreeNode : Document<TreeNode>, IHasAncestry<TreeNode>
	{
		public string Name { get; set; }

		private AncestryProxy<TreeNode> _ancestry;
		public AncestryProxy<TreeNode> Ancestry
		{
			get { return _ancestry ?? (_ancestry = new AncestryProxy<TreeNode>(this)); }
		}
	}
}