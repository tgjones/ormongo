using System;
using MongoDB.Bson.DefaultSerializer;

namespace Ormongo.Internal
{
	public class DocumentClassMap : BsonClassMap
	{
		public DocumentClassMap(Type classType)
			: base(classType)
		{
		}
	}
}