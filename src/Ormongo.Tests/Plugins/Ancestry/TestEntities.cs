using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins.Ancestry
{
	public class TreeNode : OrderedAncestryDocument<TreeNode>
	{
		public string Name { get; set; }
	}

	public class FolderNode : TreeNode
	{

	}

	public class FileNode : TreeNode
	{

	}
}