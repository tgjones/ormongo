using System;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class OrmongoConfigurationTests
	{
		[Test]
		public void CanDropDatabase()
		{
			// Arrange.
			BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "Blog post title",
				Text = "My blog post"
			});
			Assert.That(OrmongoConfiguration.GetMongoServer().DatabaseExists(OrmongoConfiguration.Database), Is.True);

			// Act.
			OrmongoConfiguration.DropDatabase();

			// Assert.
			Assert.That(OrmongoConfiguration.GetMongoServer().DatabaseExists(OrmongoConfiguration.Database), Is.False);
		}
	}
}