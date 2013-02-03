using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class DocumentTests : TestsBase
	{
		private static BlogPost CreateBlogPost()
		{
			return new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post",
				Text = "Some text"
			};
		}

		[Test]
		public void DefaultScope()
		{
			// Arrange.
			var defaultScope = BlogPost.DefaultScope;
			BlogPost.DefaultScope = items => items.OrderBy(d => d.Title);
			BlogPost post1 = BlogPost.Create(new BlogPost
			{
				Title = "Z",
			});
			BlogPost post2 = BlogPost.Create(new BlogPost
			{
				Title = "A",
			});

			// Act.
			BlogPost retrievedPost = BlogPost.All().First();

			// Assert.
			Assert.That(retrievedPost.ID, Is.EqualTo(post2.ID));

			// Clean up.
			BlogPost.DefaultScope = defaultScope;
		}

		[Test]
		public void CanCreateNewBlogPost()
		{
			// Act.
			BlogPost post = BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post",
				Text = "Some text"
			});

			// Assert.
			Assert.That(post.ID, Is.Not.EqualTo(ObjectId.Empty));
		}

		[Test]
		public void CanDestroyBlogPost()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			post.Destroy();

			// Assert.
			Assert.That(post.IsDestroyed, Is.True);
		}

		[Test]
		public void CanDeleteBlogPost()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			BlogPost.Delete(post.ID);

			// Assert.
			Assert.That(BlogPost.All().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CanDeleteAllBlogPost()
		{
			// Arrange.
			BlogPost post1 = CreateBlogPost();
			post1.Save();
			BlogPost post2 = CreateBlogPost();
			post2.Save();

			// Act.
			Assert.That(BlogPost.All().Count(), Is.EqualTo(2));
			BlogPost.DeleteAll();

			// Assert.
			Assert.That(BlogPost.All().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CanFindBlogPostsByPredicate()
		{
			// Arrange.
			BlogPost post1 = CreateBlogPost();
			post1.Save();
			BlogPost post2 = CreateBlogPost();
			post2.Save();

			// Act.
			var blogPosts = BlogPost.Where(u => u.Title == "My Blog Post");

			// Assert.
			Assert.That(blogPosts, Is.Not.Null);
			Assert.That(blogPosts.ToList(), Has.Count.EqualTo(2));
		}

		[Test]
		public void CanFindBlogPostByPredicate()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			BlogPost myPost = BlogPost.Find(u => u.Title == "My Blog Post");

			// Assert.
			Assert.That(myPost, Is.Not.Null);
			Assert.That(myPost.Text, Is.EqualTo("Some text"));
		}

		[Test]
		public void FindOneReturnsNullIfNoMatch()
		{
			// Act.
			BlogPost myPost = BlogPost.Find(u => u.Title == "My Blog Post");

			// Assert.
			Assert.That(myPost, Is.Null);
		}

		[Test]
		public void CanFindBlogPostByID()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			BlogPost myPost = BlogPost.Find(post.ID);

			// Assert.
			Assert.That(myPost.Text, Is.EqualTo("Some text"));
		}

		[Test]
		public void CanFindAllBlogPosts()
		{
			// Arrange.
			BlogPost post1 = CreateBlogPost();
			post1.Save();
			BlogPost post2 = CreateBlogPost();
			post2.Save();

			// Act.
			var allPosts = BlogPost.All();

			// Assert.
			Assert.That(allPosts.Count(), Is.EqualTo(2));
		}

		[Test]
		public void CanFindBlogPostsByAuthor()
		{
			// Arrange.
			var authorID = ObjectId.GenerateNewId();
			BlogPost post1 = CreateBlogPost();
			post1.Authors = new System.Collections.Generic.List<ObjectId> { authorID };
			post1.Save();
			BlogPost post2 = CreateBlogPost();
			post2.Save();

			// Act.
			var matchingPosts = BlogPost.Where(bp => bp.Authors.Contains(authorID)).QueryDump(Log).ToList();

			// Assert.
			Assert.That(matchingPosts.Count, Is.EqualTo(1));
		}

		#region Atomic

		[Test]
		public void CanPushComment()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			var comment = new Comment
			{
				Date = DateTime.Now,
				Name = "Tim Jones"
			};
			BlogPost.Push(post.ID, p => p.Comments, comment);

			// Assert.
			BlogPost myPost = BlogPost.Find(post.ID);
			Assert.That(myPost.Comments.Count, Is.EqualTo(1));
			Assert.That(myPost.Comments[0].Name, Is.EqualTo("Tim Jones"));
		}

		[Test]
		public void CanPullComment()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Comments.Add(new Comment
			{
				Date = DateTime.Now,
				Name = "Tim Jones"
			});
			post.Save();

			// Act.
			BlogPost.Pull(post.ID, p => p.Comments, c => c.Name == "Tim Jones");

			// Assert.
			BlogPost myPost = BlogPost.Find(post.ID);
			Assert.That(myPost.Comments.Count, Is.EqualTo(0));
		}

		[Test]
		public void IncUpdatesDatabaseAndLocalValuesForSimpleProperty()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.ReadCount = 3;
			post.Save();

			// Act.
			post.Inc(p => p.ReadCount, 1);

			// Assert.
			Assert.That(post.ReadCount, Is.EqualTo(4));
			Assert.That(BlogPost.Find(post.ID).ReadCount, Is.EqualTo(4));
		}

		#endregion

		[Test]
		public void CanSaveNewBlogPost()
		{
			// Act.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Assert.
			Assert.That(post.ID, Is.Not.EqualTo(ObjectId.Empty));
		}

		[Test]
		public void CanSaveExistingBlogPost()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			post.Text = "My New Title";
			post.Save();

			// Assert.
			Assert.That(BlogPost.All().Count(), Is.EqualTo(1));
		}

		[Test]
		public void CanEnsureIndexOnSingleKey()
		{
			// Arrange.
			OrmongoConfiguration.AutoCreateIndexes = true;

			// Act.
			BlogPost.EnsureIndex(bp => bp.Text);

			// Assert.
			Assert.That(BlogPost.GetCollection().IndexExistsByName("Text_1"), Is.True);
		}

		[Test]
		public void CanEnsureIndexOnMultipleKeysOfSameType()
		{
			// Arrange.
			OrmongoConfiguration.AutoCreateIndexes = true;

			// Act.
			BlogPost.EnsureIndex(bp => bp.Text, bp => bp.Title);

			// Assert.
			Assert.That(BlogPost.GetCollection().IndexExistsByName("Text_1_Title_1"), Is.True);
		}

		[Test]
		public void CanEnsureIndexOnMultipleKeysOfDifferentTypes()
		{
			// Arrange.
			OrmongoConfiguration.AutoCreateIndexes = true;

			// Act.
			BlogPost.EnsureIndex(bp => bp.Text, bp => bp.DatePublished);

			// Assert.
			Assert.That(BlogPost.GetCollection().IndexExistsByName("Text_1_DatePublished_1"), Is.True);
		}

		#region Inheritance

		public abstract class Animal : Document<Animal>
		{
			
		}

		public class Dog : Animal
		{
			
		}

		[Test]
		public void InheritedClassesGetSavedIntoRootClassCollection()
		{
			// Act.
			var dog = Animal.Create(new Dog());

			// Assert.
			Assert.That(Animal.Find(dog.ID), Is.Not.Null);
			Assert.That(Animal.Find(dog.ID), Is.InstanceOf<Dog>());
		}

		#endregion

		#region Equality

		[Test]
		public void ObjectsWithSameIDAreEqualUsingEqualsMethod()
		{
			// Arrange.
			var id = ObjectId.GenerateNewId();
			var post1 = new BlogPost { ID = id };
			var post2 = new BlogPost { ID = id };

			// Act / Assert.
			Assert.That(post1.Equals(post2), Is.True);
		}

		[Test]
		public void ObjectsWithSameIDAreEqualUsingEqualityOperator()
		{
			// Arrange.
			var id = ObjectId.GenerateNewId();
			var post1 = new BlogPost { ID = id };
			var post2 = new BlogPost { ID = id };

			// Act / Assert.
			Assert.That(post1 == post2, Is.True);
			Assert.That(post1 != post2, Is.False);
		}

		[Test]
		public void ObjectsWithEmptyIDsAreNotEqualUsingEqualsMethod()
		{
			// Arrange.
			var post1 = new BlogPost();
			var post2 = new BlogPost();

			// Act / Assert.
			Assert.That(post1.Equals(post2), Is.False);
		}

		[Test]
		public void ObjectsWithEmptyIDsAreNotEqualUsingEqualityOperator()
		{
			// Arrange.
			var post1 = new BlogPost();
			var post2 = new BlogPost();

			// Act / Assert.
			Assert.That(post1 == post2, Is.False);
		}

		#endregion

		#region Lazy loading

		[Test, ExpectedException]
		public void ReferencesOneRelationsThrowExceptionIfRelationIsNotSavedFirst()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Graham",
				LastName = "Greene"
			};

			// Act.
			Book.Create(new Book
			{
				Title = "The Quiet American",
				Author = person
			});
		}

		[Test]
		public void ReferencesOneRelationsOnlySerializeIDToDatabase()
		{
			// Arrange.
			var person = Person.Create(new Person
			{
				FirstName = "Graham",
				LastName = "Greene"
			});
			var book = Book.Create(new Book
			{
				Title = "The Quiet American",
				Author = person
			});

			// Act.
			var result = Book.GetCollection().FindOneAs<BsonDocument>(Query.EQ("_id", book.ID));

			// Assert.
			Assert.That(result["Author"].AsObjectId, Is.EqualTo(person.ID));
		}

		private class BookWithNonVirtualAuthor : Document<BookWithNonVirtualAuthor>
		{
			public string Title { get; set; }
			public Person Author { get; set; }
		}

		[Test, ExpectedException]
		public void RelationalAssociationsMustBeDeclaredVirtual()
		{
			// Arrange.
			var person = Person.Create(new Person
			{
				FirstName = "Graham",
				LastName = "Greene"
			});

			// Act.
			BookWithNonVirtualAuthor.Create(new BookWithNonVirtualAuthor
			{
				Title = "The Quiet American",
				Author = person
			});
		}

		[Test]
		public void ReferencesOneRelationsAreLazyLoaded()
		{
			// Arrange.
			var person = Person.Create(new Person
			{
				FirstName = "Graham",
				LastName = "Greene"
			});
			var book = Book.Create(new Book
			{
				Title = "The Quiet American",
				Author = person
			});

			// Act.
			var retrievedBook = Book.Find(book.ID);

			// Assert.
			Assert.That(retrievedBook.Author, Is.EqualTo(person));
		}

		[Test]
		public void ReferencesOneWitNullReferenceWorks()
		{
			// Arrange.
			var book = Book.Create(new Book
			{
				Title = "The Quiet American",
				Author = null
			});

			// Act.
			var retrievedBook = Book.Find(book.ID);

			// Assert.
			Assert.That(retrievedBook.Author, Is.Null);
		}

		[Test]
		public void ProxyObjectsAreSerialisedAsOriginalType()
		{
			// Arrange.
			var book = Book.Create(new Book
			{
				Title = "The Quiet American",
				Author = null
			});

			// Act.
			var retrievedBook = Book.Find(book.ID);
			retrievedBook.Save();

			// Assert.
			Assert.That(Book.All().Count(), Is.EqualTo(1));
			Assert.That(
				Book.GetCollection().FindOneAs<BsonDocument>(Query.EQ("_id", book.ID))["_t"].AsString, 
				Is.EqualTo("Book"));
		}

		[Test, ExpectedException]
		public void ReferencesManyAssociationsMustBeDeclaredVirtual()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Quiet American" });

			// Act.
			AuthorWithNonVirtualBooks.Create(new AuthorWithNonVirtualBooks
			{
				FirstName = "Graham",
				LastName = "Greene",
				Books = new List<Book> { book }
			});
		}

		[Test]
		public void ReferencesManyRelationsOnlySerializeIDsToDatabase()
		{
			// Arrange.
			var book1 = Book.Create(new Book { Title = "The Quiet American" });
			var book2 = Book.Create(new Book { Title = "The Great Gatsby" });
			var author = Author.Create(new Author
			{
				FirstName = "Graham",
				LastName = "Greene",
				Books = new List<Book> { book1, book2 }
			});

			// Act.
			var result = Author.GetCollection().FindOneAs<BsonDocument>(Query.EQ("_id", author.ID));

			// Assert.
			Assert.That(result["Books"].AsBsonArray.RawValues, Contains.Item(book1.ID));
			Assert.That(result["Books"].AsBsonArray.RawValues, Contains.Item(book2.ID));
		}

		[Test]
		public void ReferencesManyRelationsAreLazyLoaded()
		{
			// Arrange.
			var book1 = Book.Create(new Book { Title = "The Quiet American" });
			var book2 = Book.Create(new Book { Title = "The Great Gatsby" });
			var author = Author.Create(new Author
			{
				FirstName = "Graham",
				LastName = "Greene",
				Books = new List<Book> { book1, book2 }
			});

			// Act.
			var retrievedAuthor = (Author) Author.Find(author.ID);

			// Assert.
			Assert.That(retrievedAuthor.Books, Is.Not.Null);
			Assert.That(retrievedAuthor.Books, Contains.Item(book1));
			Assert.That(retrievedAuthor.Books, Contains.Item(book2));
		}

		[Test]
		public void CanGetUnderlyingTypeOfDocumentProxy()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Quiet American" });

			// Act.
			var retrievedBook = Book.Find(book.ID);
			var retrievedBookProxy = retrievedBook as IProxy;

			// Assert.
			Assert.That(retrievedBookProxy, Is.Not.Null);
			Assert.That(retrievedBookProxy.GetUnderlyingType(), Is.EqualTo(typeof(Book)));
		}

		[Test]
		public void ReferencesManyWitNullReferenceWorks()
		{
			// Arrange.
			var author = Author.Create(new Author
			{
				FirstName = "Graham",
				LastName = "Greene",
				Books = null
			});

			// Act.
			var retrievedAuthor = (Author)Author.Find(author.ID);

			// Assert.
			Assert.That(retrievedAuthor.Books, Is.Null);
		}

		[Test]
		public void ReferencesManyRelationProxiesCanBeSaved()
		{
			// Arrange.
			var book1 = Book.Create(new Book { Title = "The Quiet American" });
			var book2 = Book.Create(new Book { Title = "The Great Gatsby" });
			var author = Author.Create(new Author
			{
				FirstName = "Graham",
				LastName = "Greene",
				Books = new List<Book> { book1, book2 }
			});

			// Act.
			var retrievedAuthor = (Author)Author.Find(author.ID);
			retrievedAuthor.Books = new List<Book>
			{
				retrievedAuthor.Books[1],
				retrievedAuthor.Books[0]
			};
			retrievedAuthor.Save();

			// Assert.
			var retrievedAuthor2 = (Author)Author.Find(author.ID);
			Assert.That(retrievedAuthor2.Books, Is.Not.Null);
			Assert.That(retrievedAuthor2.Books, Contains.Item(book1));
			Assert.That(retrievedAuthor2.Books, Contains.Item(book2));
		}

		#endregion

		#region Dirty tracking

		[Test]
		public void PropertiesAreNotFlaggedAsChangedInitially()
		{
			// Arrange.
			var book = new Book();

			// Act / Assert.
			Assert.That(book.HasValueChanged(b => b.Title), Is.False);
		}

		[Test]
		public void PropertiesAreFlaggedAsChanged()
		{
			// Arrange.
			var book = new Book { Title = "The Loud American" };

			// Act / Assert.
			Assert.That(book.HasValueChanged(b => b.Title), Is.True);
		}

		[Test]
		public void PropertiesAreNotFlaggedAsChangedAfterSave()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Loud American" });

			// Act / Assert.
			Assert.That(book.HasValueChanged(b => b.Title), Is.False);
		}

		[Test]
		public void ChangingValueAfterSavingFlagsPropertyAsChanged()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Loud American" });
			book.Title = "The Quiet American";

			// Act / Assert.
			Assert.That(book.HasValueChanged(b => b.Title), Is.True);
		}

		[Test]
		public void DocumentLoadedFromDatabaseDoesNotFlagPropertiesAsChanged()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Loud American" });
			var retrievedBook = Book.Find(book.ID);

			// Act / Assert.
			Assert.That(retrievedBook.HasValueChanged(b => b.Title), Is.False);
		}

		[Test]
		public void CanGetOriginalValueOfProperty()
		{
			// Arrange.
			var book = Book.Create(new Book { Title = "The Loud American" });
			book.Title = "The Quiet American";

			// Act.
			var result = book.GetOriginalValue(b => b.Title);

			// Act.
			Assert.That(result, Is.EqualTo("The Loud American"));
		}

		#endregion

		#region Validation

		[Test]
		public void IsValidReturnsTrueForValidDocument()
		{
			// Arrange.
			var person = new Person { FirstName = "Tim" };

			// Act / Assert.
			Assert.That(person.IsValid, Is.True);
		}

		[Test]
		public void IsValidReturnsFalseForInvalidDocument()
		{
			// Arrange.
			var person = new Person { FirstName = null };

			// Act / Assert.
			Assert.That(person.IsValid, Is.False);
		}

		[Test]
		public void ValidationIncludesEmbeddedDocumentValidators()
		{
			// Arrange.
			var person = new Person
			{
				FirstName = "Tim",
				Address = new Address
				{
					City = null
				}
			};

			// Act / Assert.
			Assert.That(person.IsValid, Is.False);
		}

		#endregion
	}
}