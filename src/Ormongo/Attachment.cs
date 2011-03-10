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

		public Attachment(Stream content, string fileName, string contentType)
		{
			Content = content;
			FileName = fileName;
			ContentType = contentType;
			IsLoaded = true;
		}

		internal Attachment(ObjectId id, MongoGridFSFileInfo fileInfo)
		{
			ID = id;
			Load(fileInfo);
		}

		internal Attachment(ObjectId id)
		{
			ID = id;
			IsLoaded = false;
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

			IsLoaded = true;
		}

		internal static MongoGridFS GetGridFS()
		{
			return OrmongoConfiguration.GetMongoDatabase().GridFS;
		}

		public static void Delete(ObjectId id)
		{
			GetGridFS().DeleteById(id);
		}

		public static IEnumerable<Attachment> FindAll()
		{
			return GetGridFS().Files.FindAll().Select(f => new Attachment(f["_id"].AsObjectId));
		}

		public static Attachment FindByID(ObjectId id)
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
				UploadDate = DateTime.UtcNow
			};
			gridFS.Upload(Content, FileName, options);
		}
	}
}