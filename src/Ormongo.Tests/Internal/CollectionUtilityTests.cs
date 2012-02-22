using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class CollectionUtilityTests
	{
		[Test]
		public void CanGetCollectionName()
		{
			Assert.That(CollectionUtility.GetCollectionName(typeof(Book)), Is.EqualTo("Book"));
		}

		private class Novel : Book
		{

		}

		[Test]
		public void CanGetCollectionNameForInheritedClasses()
		{
			Assert.That(CollectionUtility.GetCollectionName(typeof(Novel)), Is.EqualTo("Book"));
		}
	}
}