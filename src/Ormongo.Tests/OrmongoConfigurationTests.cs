using System;
using MongoDB.Driver;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class OrmongoConfigurationTests
	{
		[Test]
		public void CanCloseConnection()
		{
			// Arrange.
			BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "Blog post title",
				Text = "My blog post"
			});
			Assert.That(OrmongoConfiguration.GetMongoServer().State, Is.EqualTo(MongoServerState.Connected));

			// Act.
			OrmongoConfiguration.CloseConnection();

			// Assert.
			Assert.That(OrmongoConfiguration.GetMongoServer().State, Is.EqualTo(MongoServerState.Disconnected));
		}

		[Test]
		public void CanDropAllCollections()
		{
			// Arrange.
			BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "Blog post title",
				Text = "My blog post"
			});
			Assert.That(OrmongoConfiguration.GetMongoDatabase().CollectionExists("BlogPost"), Is.True);

			// Act.
			OrmongoConfiguration.DropAllCollections();

			// Assert.
			Assert.That(OrmongoConfiguration.GetMongoDatabase().CollectionExists("BlogPost"), Is.False);
		}
	}
}