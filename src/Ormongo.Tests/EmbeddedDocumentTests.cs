using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class EmbeddedDocumentTests
	{
		[TearDown]
		public void TearDown()
		{
			Person.Drop();
		}

		[Test]
		public void CanManuallyUpdateEmbeddedDocumentParentReferences()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Tim",
				LastName = "Jones"
			};
			var address = new Address
			{
				Street = "1 Recursive Loop",
				City = "San Francisco",
				State = "California",
				Postcode = "1(1(1))"
			};

			// Act.
			person.Address = address;
			person.UpdateAssociations();

			// Assert.
			Assert.That(address.Parent, Is.EqualTo(person));
		}

		[Test]
		public void CanSaveAndLoadNullEmbeddedDocument()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Tim",
				LastName = "Jones",
				Address = null
			};

			// Act.
			person.Save();

			// Assert.
			Assert.That(Person.GetCollection().Count(), Is.EqualTo(1));
			Assert.That(Person.GetCollection().FindOne().ID, Is.EqualTo(person.ID));
			Assert.That(Person.GetCollection().FindOne().Address, Is.Null);
		}

		[Test]
		public void CanSaveEmbeddedDocument()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Tim",
				LastName = "Jones"
			};
			var address = new Address
			{
				Street = "1 Recursive Loop",
				City = "San Francisco",
				State = "California",
				Postcode = "1(1(1))"
			};

			// Act.
			person.Address = address;
			person.Save();

			// Assert.
			Assert.That(address.Parent, Is.EqualTo(person));
			Assert.That(Person.GetCollection().Count(), Is.EqualTo(1));
			Assert.That(Person.GetCollection().FindOne().ID, Is.EqualTo(person.ID));
			Assert.That(Person.GetCollection().FindOne().Address.ID, Is.EqualTo(person.Address.ID));
		}

		[Test]
		public void CanLoadEmbeddedDocument()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Tim",
				LastName = "Jones"
			};
			var address = new Address
			{
				Street = "1 Recursive Loop",
				City = "San Francisco",
				State = "California",
				Postcode = "1(1(1))"
			};
			person.Address = address;
			person.Save();

			// Act.
			Person myPerson = Person.Find(person.ID);
			
			// Assert.
			Assert.That(myPerson.Address.Parent, Is.EqualTo(myPerson));
		}
	}
}