using System;
using MongoDB.Bson;

namespace Ormongo
{
	public class AttachmentDeletingEventArgs : EventArgs
	{
		public ObjectId AttachmentID { get; private set; }

		public AttachmentDeletingEventArgs(ObjectId attachmentID)
		{
			AttachmentID = attachmentID;
		}
	}
}