using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class AssociationUtilityTests
	{
		[Test]
		public void CanUpdateAssociationsInProperty()
		{
			// Arrange.
			var person = new Person();
			var address = new Address();

			// Act.
			person.Address = address;
			EmbeddedDocumentUtility.UpdateParentReferences(person);

			// Assert.
			Assert.That(address.Parent, Is.EqualTo(person));
		}

		[Test]
		public void CanUpdateAssociationsInCollection()
		{
			// Arrange.
			var blogPost = new BlogPost();
			var comment = new Comment();

			// Act.
			blogPost.Comments.Add(comment);
			EmbeddedDocumentUtility.UpdateParentReferences(blogPost);

			// Assert.
			Assert.That(comment.Parent, Is.EqualTo(blogPost));
		}
	}
}