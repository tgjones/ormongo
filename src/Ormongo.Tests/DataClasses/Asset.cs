namespace Ormongo.Tests.DataClasses
{
	public class Asset : Document<Asset>
	{
		public string Title { get; set; }
		public Attachment File { get; set; }
	}
}