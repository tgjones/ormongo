using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Ormongo.Validation;

namespace Ormongo.Tests.Validation
{
	[TestFixture]
	public class DocumentUniquenessValidatorTests
	{
		public class TestEntity : Document<TestEntity>
		{
			public int SiteID { get; set; }
			public string Title { get; set; }
		}

		public class Site : Document<Site>
		{
			public string Title { get; set; }
		}

		public class TestEntityWithReference : Document<TestEntityWithReference>
		{
			public virtual Site Site { get; set; }
			public string Title { get; set; }
		}

		[SetUp]
		public void SetUp()
		{
			TestEntity.Drop();
			Site.Drop();
			TestEntityWithReference.Drop();
		}

		[Test]
		public void SucceedsForValidData()
		{
			// Arrange.
			var validator = new DocumentUniquenessValidator<TestEntity, string>(x => x.Title)
			{
				Scope = new Expression<Func<TestEntity, object>>[] { x => x.SiteID }
			};
			TestEntity.Create(new TestEntity { SiteID = 1, Title = "Foo" });
			var documentInstance = new TestEntity { SiteID = 2, Title = "Foo" };
			var context = DocumentValidationContext<TestEntity>.Create(documentInstance, SaveType.Any);

			// Act.
			var results = validator.Validate("Foo", context).ToList();

			// Assert.
			Assert.That(results, Is.Empty);
		}

		[Test]
		public void FailsForInvalidData()
		{
			// Arrange.
			var validator = new DocumentUniquenessValidator<TestEntity, string>(x => x.Title)
			{
				Scope = new Expression<Func<TestEntity, object>>[] { x => x.SiteID }
			};
			TestEntity.Create(new TestEntity { SiteID = 1, Title = "Foo" });
			var documentInstance = new TestEntity { SiteID = 1, Title = "Foo" };
			var context = DocumentValidationContext<TestEntity>.Create(documentInstance, SaveType.Any);

			// Act.
			var results = validator.Validate("Foo", context).ToList();

			// Assert.
			Assert.That(results, Has.Count.EqualTo(1));
		}

		[Test]
		public void SucceedsForValidDataWithReference()
		{
			// Arrange.
			var validator = new DocumentUniquenessValidator<TestEntityWithReference, string>(x => x.Title)
			{
				Scope = new Expression<Func<TestEntityWithReference, object>>[] { x => x.Site }
			};
			TestEntityWithReference.Create(new TestEntityWithReference
			{
				Site = Site.Create(new Site { Title = "Baz" }),
				Title = "Foo"
			});
			var documentInstance = new TestEntityWithReference
			{
				Site = Site.Create(new Site { Title = "Bar" }),
				Title = "Foo"
			};
			var context = DocumentValidationContext<TestEntityWithReference>.Create(documentInstance, SaveType.Any);

			// Act.
			var results = validator.Validate("Foo", context).ToList();

			// Assert.
			Assert.That(results, Is.Empty);
		}

		[Test]
		public void FailsForInvalidDataWithReference()
		{
			// Arrange.
			var validator = new DocumentUniquenessValidator<TestEntityWithReference, string>(x => x.Title)
			{
				Scope = new Expression<Func<TestEntityWithReference, object>>[] { x => x.Site }
			};
			var site = Site.Create(new Site { Title = "Bar" });
			TestEntityWithReference.Create(new TestEntityWithReference
			{
				Site = site,
				Title = "Foo"
			});
			var documentInstance = new TestEntityWithReference
			{
				Site = site,
				Title = "Foo"
			};
			var context = DocumentValidationContext<TestEntityWithReference>.Create(documentInstance, SaveType.Any);

			// Act.
			var results = validator.Validate("Foo", context).ToList();

			// Assert.
			Assert.That(results, Has.Count.EqualTo(1));
		}
	}
}