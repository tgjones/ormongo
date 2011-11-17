using System.Collections.Generic;

namespace Ormongo
{
	public class DatabaseStatistics
	{
		public double AverageObjectSize { get; set; }
		public int CollectionCount { get; set; }
		public long DataSize { get; set; }
		public int ExtentCount { get; set; }
		public long FileSize { get; set; }
		public int IndexCount { get; set; }
		public long IndexSize { get; set; }
		public long ObjectCount { get; set; }
		public long StorageSize { get; set; }
		public IEnumerable<CollectionStatistics> CollectionStatistics { get; set; }
	}
}