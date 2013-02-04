using MongoDB.Bson;

namespace Ormongo.Internal
{
	public static class DocumentUtility
	{
		public static ObjectId GetDocumentID(object document)
		{
			// TODO: Cache this reflection.
			return (ObjectId) document.GetType().GetProperty("ID").GetValue(document, null);
		} 
	}
}