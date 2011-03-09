using System;
using System.Collections.Generic;

namespace Ormongo.Tests.DataClasses
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

	public class Comment : EmbeddedDocument
	{
		public DateTime Date { get; set; }
		public string Name { get; set; }
	}
}