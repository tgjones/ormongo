using System.Collections.Generic;
using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class ReflectionUtilityTests
	{
		[Test]
		public void CanCheckSubclassOfRawGeneric()
		{
			// Arrange.
			var myList = new List<int>();

			// Act.
			bool result = ReflectionUtility.IsSubclassOfRawGeneric(typeof(List<>), myList.GetType());

			// Assert.
			Assert.That(result, Is.True);
		}

		[Test]
		public void CanCheckSubclassOfReferencedDocumentCollection()
		{
			// Arrange.
			var myList = new ReferencedDocumentCollection<BlogPost, Person>(null, null);

			// Act.
			bool result = ReflectionUtility.IsSubclassOfRawGeneric(typeof(List<>), myList.GetType());

			// Assert.
			Assert.That(result, Is.True);
		}

		private class Book : Document<Book>
		{
			
		}

		private class Novel : Book
		{

		}

		[Test]
		public void CanGetTypeOfRawGeneric()
		{
			// Act.
			var result = ReflectionUtility.GetTypeOfRawGeneric(typeof(Document<>), typeof(Novel));

			// Assert.
			Assert.That(result, Is.EqualTo(typeof(Book)));
		}
	}
}