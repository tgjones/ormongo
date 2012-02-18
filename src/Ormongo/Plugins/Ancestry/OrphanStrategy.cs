namespace Ormongo.Plugins.Ancestry
{
	public enum OrphanStrategy
	{
		/// <summary>
		/// All children are destroyed as well (default)
		/// </summary>
		Destroy,

		/// <summary>
		/// The children of the destroyed node become root nodes
		/// </summary>
		Rootify,

		/// <summary>
		/// An exception is thrown if any children exist
		/// </summary>
		Restrict
	}
}