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

		private MongoGridFSFileInfo _fileInfo;

		public ObjectId ID { get; set; }
		public bool IsPersisted { get; private set; }

		internal bool IsLoaded { get; private set; }
		internal bool IsContentLoaded {get; private set;}

		private Stream _content;
		private bool _contentChanged;
		public Stream Content
		{
			get
			{
				if (!IsContentLoaded)
					LoadContent();
				return _content;
			}
			set
			{
				_content = value;
				_contentChanged = true;
				IsContentLoaded = true;
			}
		}

		private string _fileName;
		private bool _fileNameChanged;
		public string FileName
		{
			get
			{
				if (!IsLoaded)
					Load();
				return _fileName;
			}
			set
			{
				_fileName = value;
				_fileNameChanged = true;
			}
		}

		private string _contentType;
		private bool _contentTypeChanged;
		public string ContentType
		{
			get
			{
				if (!IsLoaded)
					Load();
				return _contentType;
			}
			set
			{
				_contentType = value;
				_contentTypeChanged = true;
			}
		}

		private BsonDocument _metadata;
		public BsonDocument Metadata
		{
			get
			{
				if (!IsLoaded)
					Load();
				if (_metadata == null)
					Metadata = new BsonDocument();
				return _metadata;
			}
			set { _metadata = value; }
		}

		public Attachment()
		{
			
		}

		public Attachment(Stream content)
		{
			_content = content;
			IsLoaded = true;
			IsContentLoaded = true;
		}

		internal Attachment(ObjectId id)
		{
			ID = id;
			IsPersisted = true;
		}

		private Attachment(MongoGridFSFileInfo fileInfo)
		{
			_fileInfo = fileInfo;
			ID = fileInfo.Id.AsObjectId;
			IsPersisted = true;
		}

		public static Attachment Create(Attachment attachment)
		{
			attachment.Save();
			return attachment;
		}

		private void LoadContent()
		{
			if (!IsLoaded)
				Load();
			_content = _fileInfo.OpenRead();
			IsContentLoaded = true;
		}

		private void Load()
		{
			if (_fileInfo == null) // True when Attachment is loaded from serialised Attachment property.
				_fileInfo = GetGridFS().FindOneById(ID);
			if (_fileInfo == null)
				return;

			_contentType = _fileInfo.ContentType;
			_fileName = _fileInfo.Name;
			_metadata = _fileInfo.Metadata;

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
			//return GetGridFS().Files.FindAll().Select(f => new Attachment(f["_id"].AsObjectId));
			return GetGridFS().FindAll().Select(f => new Attachment(f));
		}

		public static Attachment Find(ObjectId id)
		{
			var file = GetGridFS().FindOneById(id);
			return (file != null) ? new Attachment(file) : null;
		}

		public void Save()
		{
			var gridFS = GetGridFS();

			if (IsPersisted)
			{
				if (!IsLoaded)
					Load();

				if (_contentChanged || _fileNameChanged)
				{
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
				else
				{
					if (_contentTypeChanged)
						gridFS.SetContentType(_fileInfo, _contentType);
					// Metadata is a BsonDocument - we can't detect if it's changed, so always save it.
					gridFS.SetMetadata(_fileInfo, _metadata);
				}
			}
			else
			{
				ID = ObjectId.GenerateNewId();

				var options = new MongoGridFSCreateOptions
				{
					ContentType = _contentType,
					Id = ID,
					UploadDate = DateTime.UtcNow,
					Metadata = _metadata
				};
				gridFS.Upload(_content, _fileName, options);

				IsPersisted = true;
			}

			_contentChanged = false;
			_fileNameChanged = false;
			_contentTypeChanged = false;
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