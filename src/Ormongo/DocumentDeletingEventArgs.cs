using System;
using MongoDB.Bson;

namespace Ormongo
{
	public class DocumentDeletingEventArgs : EventArgs
	{
		public ObjectId DocumentID { get; private set; }

		public DocumentDeletingEventArgs(ObjectId documentID)
		{
			DocumentID = documentID;
		}
	}
}