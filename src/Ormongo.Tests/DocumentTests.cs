using System;
using System.Linq;
using FluentMongo.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class DocumentTests : TestsBase
	{
		[TearDown]
		public void TearDown()
		{
			BlogPost.Drop();
		}

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
		public void TransientDataIsNotSaved()
		{
			// Arrange.
			BlogPost post = BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post",
				Text = "Some text"
			});
			post.TransientData["Hello"] = "World";
			post.Save();

			// Act.
			BlogPost retrievedPost = BlogPost.FindOneByID(post.ID);

			// Assert.
			Assert.That(retrievedPost.TransientData, Is.Empty);
		}

		[Test]
		public void ExtraDataIsSaved()
		{
			// Arrange.
			BlogPost post = BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post",
				Text = "Some text"
			});
			post.ExtraData["Hello"] = "World";
			post.Save();

			// Act.
			BlogPost retrievedPost = BlogPost.FindOneByID(post.ID);

			// Assert.
			Assert.That(retrievedPost.ExtraData.SafeGet<string>("Hello"), Is.EqualTo("World"));
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
		public void CanDeleteBlogPost()
		{
			// Arrange.
			BlogPost post = CreateBlogPost();
			post.Save();

			// Act.
			BlogPost.Delete(post.ID);

			// Assert.
			Assert.That(BlogPost.FindAll().Count(), Is.EqualTo(0));
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
			Assert.That(BlogPost.FindAll().Count(), Is.EqualTo(2));
			BlogPost.DeleteAll();

			// Assert.
			Assert.That(BlogPost.FindAll().Count(), Is.EqualTo(0));
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
			var blogPosts = BlogPost.Find(u => u.Title == "My Blog Post");

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
			BlogPost myPost = BlogPost.FindOne(u => u.Title == "My Blog Post");

			// Assert.
			Assert.That(myPost, Is.Not.Null);
			Assert.That(myPost.Text, Is.EqualTo("Some text"));
		}

		[Test]
		public void FindOneReturnsNullIfNoMatch()
		{
			// Act.
			BlogPost myPost = BlogPost.FindOne(u => u.Title == "My Blog Post");

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
			BlogPost myPost = BlogPost.FindOneByID(post.ID);

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
			var allPosts = BlogPost.FindAll();

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
			var matchingPosts = BlogPost.Find(bp => bp.Authors.Contains(authorID)).QueryDump(Log).ToList();

			// Assert.
			Assert.That(matchingPosts.Count, Is.EqualTo(1));
		}

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
			BlogPost myPost = BlogPost.FindOneByID(post.ID);
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
			BlogPost myPost = BlogPost.FindOneByID(post.ID);
			Assert.That(myPost.Comments.Count, Is.EqualTo(0));
		}

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
			Assert.That(BlogPost.FindAll().Count(), Is.EqualTo(1));
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

		private abstract class Animal : Document<Animal>
		{
			
		}

		private class Dog : Animal
		{
			
		}

		[Test]
		public void InheritedClassesGetSavedIntoRootClassCollection()
		{
			// Act.
			var dog = Animal.Create(new Dog());

			// Assert.
			Assert.That(Animal.FindOneByID(dog.ID), Is.Not.Null);
			Assert.That(Animal.FindOneByID(dog.ID), Is.InstanceOf<Dog>());
		}

		#endregion
	}
}