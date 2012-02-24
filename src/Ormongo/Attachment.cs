using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace Ormongo
{
	public class Attachment
	{
		public static EventHandler<AttachmentDeletingEventArgs> Deleting;

		public ObjectId ID { get; set; }

		public bool IsLoaded { get; private set; }

		private Stream _content;
		public Stream Content
		{
			get
			{
				if (!IsLoaded)
					Load();
				return _content;
			}
			set { _content = value; }
		}

		private string _fileName;
		public string FileName
		{
			get
			{
				if (!IsLoaded)
					Load(); 
				return _fileName;
			}
			set { _fileName = value; }
		}

		private string _contentType;
		public string ContentType
		{
			get
			{
				if (!IsLoaded)
					Load(); 
				return _contentType;
			}
			set { _contentType = value; }
		}

		private BsonDocument _metadata;
		public BsonDocument Metadata
		{
			get
			{
				if (!IsLoaded)
					Load();
				return _metadata;
			}
		}

		private Attachment(Stream content, string fileName, string contentType, BsonDocument metadata)
		{
			Content = content;
			FileName = fileName;
			ContentType = contentType;
			_metadata = metadata;
			IsLoaded = true;
		}

		private Attachment(ObjectId id, MongoGridFSFileInfo fileInfo)
		{
			ID = id;
			Load(fileInfo);
		}

		internal Attachment(ObjectId id)
		{
			ID = id;
			IsLoaded = false;
		}

		public static Attachment Create(Stream content, string fileName, string contentType, BsonDocument metadata = null)
		{
			var result = new Attachment(content, fileName, contentType, metadata);
			result.Save();
			return result;
		}

		public static Attachment Create(string fileName, string contentType, BsonDocument metadata = null)
		{
			var fileInfo = new FileInfo(fileName);
			using (var stream = fileInfo.OpenRead())
			{
				var result = new Attachment(stream, fileInfo.Name, contentType, metadata);
				result.Save();
				return result;
			}
		}

		private void Load()
		{
			var file = GetGridFS().FindOneById(ID);
			if (file != null)
				Load(file);
		}

		private void Load(MongoGridFSFileInfo fileInfo)
		{
			Content = fileInfo.OpenRead();
			ContentType = fileInfo.ContentType;
			FileName = fileInfo.Name;
			_metadata = fileInfo.Metadata;

			IsLoaded = true;
		}

		internal static MongoGridFS GetGridFS()
		{
			return OrmongoConfiguration.GetMongoDatabase().GridFS;
		}

		public static void Delete(ObjectId id)
		{
			OnDeleting(new AttachmentDeletingEventArgs(id));
			GetGridFS().DeleteById(id);
		}

		public static void DeleteAll()
		{
			GetGridFS().Chunks.RemoveAll();
			GetGridFS().Files.RemoveAll();
		}

		public static IEnumerable<Attachment> All()
		{
			return GetGridFS().Files.FindAll().Select(f => new Attachment(f["_id"].AsObjectId));
		}

		public static Attachment Find(ObjectId id)
		{
			var file = GetGridFS().FindOneById(id);
			return (file != null) ? new Attachment(id, file) : null;
		}

		public void Save()
		{
			var gridFS = GetGridFS();

			if (ID == ObjectId.Empty)
				ID = ObjectId.GenerateNewId();
			else
				gridFS.DeleteById(ID);

			var options = new MongoGridFSCreateOptions
			{
				ContentType = ContentType,
				Id = ID,
				UploadDate = DateTime.UtcNow,
				Metadata = _metadata
			};
			gridFS.Upload(Content, FileName, options);
		}

		#region Events

		protected static void OnDeleting(AttachmentDeletingEventArgs args)
		{
			if (Deleting != null)
				Deleting(null, args);
		}

		#endregion
	}
}