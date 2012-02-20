using System;

namespace Ormongo.Plugins.Ancestry
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AncestryAttribute : Attribute
	{
		/// <summary>
		/// Configures what to do with children of a node that is destroyed
		/// </summary>
		public OrphanStrategy OrphanStrategy { get; set; }

		/// <summary>
		/// Cache the depth of each node in the ExtraData.AncestryDepth field. Defaults to false.
		/// </summary>
		public bool CacheDepth { get; set; }

		/// <summary>
		/// Enable the ordering of children within the tree. Defaults to false.
		/// </summary>
		public bool OrderingEnabled { get; set; }
	}
}