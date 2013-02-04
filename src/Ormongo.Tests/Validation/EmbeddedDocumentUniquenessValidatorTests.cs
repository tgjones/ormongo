using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ormongo.Validation;

namespace Ormongo.Tests.Validation
{
	[TestFixture]
	public class EmbeddedDocumentUniquenessValidatorTests
	{
		private class TestAuthor : Document<TestAuthor>
		{
			public List<TestBook> Books { get; set; }
		}

		private class TestBook : EmbeddedDocument<TestBook, TestAuthor>
		{
			public string Title { get; set; }
		}

		[Test]
		public void SucceedsForValidData()
		{
			// Arrange.
			var validator = new EmbeddedDocumentUniquenessValidator<TestBook, TestAuthor, string>(x => x.Title);
			var testBook = new TestBook { Title = "Bar" };
			var documentInstance = new TestAuthor
			{
				Books = new List<TestBook>
				{
					new TestBook { Title = "Foo" },
					testBook
				}
			};
			var context = DocumentValidationContext<TestBook>.Create(testBook, documentInstance, "Books", SaveType.Any);

			// Act.
			var results = validator.Validate("Bar", context).ToList();

			// Assert.
			Assert.That(results, Is.Empty);
		}

		[Test]
		public void FailsForInvalidData()
		{
			// Arrange.
			var validator = new EmbeddedDocumentUniquenessValidator<TestBook, TestAuthor, string>(x => x.Title);
			var testBook = new TestBook { Title = "Bar" };
			var documentInstance = new TestAuthor
			{
				Books = new List<TestBook>
				{
					new TestBook { Title = "Foo" },
					testBook
				}
			};
			var context = DocumentValidationContext<TestBook>.Create(testBook, documentInstance, "Books", SaveType.Any);

			// Act.
			var results = validator.Validate("Foo", context).ToList();

			// Assert.
			Assert.That(results.Count, Is.EqualTo(1));
		}
	}
}