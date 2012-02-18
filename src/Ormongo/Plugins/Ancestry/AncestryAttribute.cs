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
		/// Cache the depth of each node in the ExtraData.AncestryDepth field (default: false)
		/// </summary>
		public bool CacheDepth { get; set; }
	}
}