using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Ormongo.Validation;

namespace Ormongo.Tests
{
	public class BlogPost : Document<BlogPost>
	{
		public DateTime DatePublished { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		public int ReadCount { get; set; }

		public List<Comment> Comments { get; set; }
		public List<ObjectId> Authors { get; set; }

		public BlogPost()
		{
			Comments = new List<Comment>();
		}
	}

	public class Comment : EmbeddedDocument<Comment, BlogPost>
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
		static Person()
		{
			Validates(x => x.FirstName).Presence();
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }

		public virtual Address Address { get; set; }
	}

	public class Address : EmbeddedDocumentWithID<Address, Person>
	{
		static Address()
		{
			Validates(x => x.City).Presence();
		}

		public string Street { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Postcode { get; set; }
	}

	public class Book : Document<Book>
	{
		public string Title { get; set; }
		public virtual Person Author { get; set; }
	}

	public class AuthorWithNonVirtualBooks : Person
	{
		public List<Book> Books { get; set; }
	}

	public class Author : Person
	{
		public virtual List<Book> Books { get; set; }
	}
}