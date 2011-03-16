using System;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class DocumentTests
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
		public void CanEnsureIndexOnMultipleKeys()
		{
			// Arrange.
			OrmongoConfiguration.AutoCreateIndexes = true;

			// Act.
			BlogPost.EnsureIndex(bp => bp.Text, bp => bp.Title);

			// Assert.
			Assert.That(BlogPost.GetCollection().IndexExistsByName("Text_1_Title_1"), Is.True);
		}
	}
}