using System;
using System.Collections.Generic;

namespace Ormongo.Tests
{
	public class BlogPost : Document<BlogPost>
	{
		public DateTime DatePublished { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }

		public List<Comment> Comments { get; set; }

		public BlogPost()
		{
			Comments = new List<Comment>();
		}
	}

	public class Comment : EmbeddedDocument<BlogPost>
	{
		public DateTime Date { get; set; }
		public string Name { get; set; }
	}

	public class Asset : Document<Asset>
	{
		public string Title { get; set; }
		public Attachment File { get; set; }
	}

	public class Person : Document<Person>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public virtual Address Address { get; set; }
	}

	public class Address : EmbeddedDocument<Person>
	{
		public string Street { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Postcode { get; set; }
	}
}