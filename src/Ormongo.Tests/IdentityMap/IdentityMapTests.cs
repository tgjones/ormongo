using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace Ormongo.Tests.IdentityMap
{
	[TestFixture]
	public class IdentityMapTests : TestsBase
	{
		[SetUp]
		public void SetUp()
		{
			var identityMapDictionary = new Hashtable();
			var identityMap = new Ormongo.IdentityMap.IdentityMap<BlogPost>(() => identityMapDictionary);
			BlogPost.Plugins.Add(identityMap);
		}

		[TearDown]
		public override void TearDown()
		{
			BlogPost.Plugins.Clear();
			base.TearDown();
		}

		[Test]
		public void LoadsSameInstanceWhenUsingFindByID()
		{
			// Arrange.
			var post = BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post",
				Text = "Some text"
			});

			// Act.
			var result = BlogPost.Find(post.ID);

			// Assert.
			Assert.That(ReferenceEquals(post, result), Is.True);
		}

		[Test]
		public void LoadsSameInstanceWhenUsingFindWithQuery()
		{
			// Arrange.
			var post1 = BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post 1",
				Text = "Some text"
			});
			BlogPost.Create(new BlogPost
			{
				DatePublished = DateTime.Now,
				Title = "My Blog Post 2",
				Text = "Some text"
			});

			// Act.
			var result = BlogPost.All().ToList();

			// Assert.
			Assert.That(ReferenceEquals(post1, result[0]), Is.True);
		}
	}
}