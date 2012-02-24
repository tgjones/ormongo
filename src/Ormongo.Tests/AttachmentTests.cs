using System.IO;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace Ormongo.Tests
{
	[TestFixture]
	public class AttachmentTests
	{
		[TearDown]
		public void TearDown()
		{
			Attachment.GetGridFS().Files.RemoveAll();
			Attachment.GetGridFS().Chunks.RemoveAll();
			Attachment.GetGridFS().Chunks.ResetIndexCache();

			Asset.Drop();
		}

		private static Attachment CreateAttachment()
		{
			using (var fs = File.OpenRead("Files/Koala.jpg"))
				return Attachment.Create(new Attachment
				{
					Content = fs,
					FileName = "Koala.jpg",
					ContentType = "image/jpg"
				});
		}

		[Test]
		public void CanDeleteAttachment()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			Assert.That(Attachment.All().Count(), Is.EqualTo(1));

			// Act.
			Attachment.Delete(file.ID);

			// Assert.
			Assert.That(Attachment.All().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CanCreateAttachmentFromStream()
		{
			Assert.That(Attachment.All().Count(), Is.EqualTo(0));

			// Act.
			Attachment file;
			using (var fs = File.OpenRead("Files/Koala.jpg"))
				file = Attachment.Create(new Attachment
				{
					Content = fs,
					ContentType = "image/jpg"
				});

			// Assert.
			Assert.That(file.ID, Is.Not.EqualTo(ObjectId.Empty));
			Assert.That(Attachment.All().Count(), Is.EqualTo(1));
		}

		[Test]
		public void CanSaveAttachment()
		{
			// Act.
			Assert.That(Attachment.All().Count(), Is.EqualTo(0));
			Attachment file = CreateAttachment();

			// Assert.
			Assert.That(file.ID, Is.Not.EqualTo(ObjectId.Empty));
			Assert.That(Attachment.All().Count(), Is.EqualTo(1));
		}

		[Test]
		public void CanSaveExistingAttachment()
		{
			// Arrange.
			using (FileStream stream = File.OpenRead("Files/Koala.jpg"))
			{
				Attachment file = Attachment.Create(new Attachment
				{
					Content = stream,
					FileName = "Files/Koala.jpg",
					ContentType = "image/jpg"
				});
				ObjectId id = file.ID;

				// Act.
				file.FileName = "newfilename.jpg";
				file.Save();

				// Assert.
				Assert.That(file.ID, Is.EqualTo(id));
				Assert.That(Attachment.All().Count(), Is.EqualTo(1));
			}
		}

		[Test]
		public void CanSaveAttachmentsWithDuplicateFileNames()
		{
			// Act.
			Assert.That(Attachment.All().Count(), Is.EqualTo(0));
			Attachment file1 = CreateAttachment();
			Attachment file2 = CreateAttachment();

			// Assert.
			Assert.That(Attachment.All().Count(), Is.EqualTo(2));
			Assert.That(Attachment.Find(file1.ID), Is.Not.Null);
			Assert.That(Attachment.Find(file2.ID), Is.Not.Null);
		}

		[Test]
		public void FindByIDReturnsNullForInvalidID()
		{
			// Assert.
			Assert.That(Attachment.Find(ObjectId.GenerateNewId()), Is.Null);
		}

		[Test]
		public void CanSaveAndLoadMetadata()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			file.Metadata["Huggable"] = true;
			file.Save();

			// Act.
			var retrievedFile = Attachment.Find(file.ID);

			// Assert.
			Assert.That(retrievedFile.Metadata, Is.Not.Null);
			Assert.That(retrievedFile.Metadata.Contains("Huggable"), Is.True);
			Assert.That(retrievedFile.Metadata["Huggable"].AsBoolean, Is.True);
		}

		[Test]
		public void CanSaveAsset()
		{
			// Arrange.
			Attachment file = CreateAttachment();

			// Act.
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Assert.
			var assetDoc = Asset.GetCollection().FindOneByIdAs<BsonDocument>(asset.ID);
			Assert.That(assetDoc["File"].AsObjectId, Is.EqualTo(file.ID));
		}

		[Test]
		public void CanLoadAndReadAsset()
		{
			using (FileStream stream = File.OpenRead("Files/Koala.jpg"))
			{
				// Arrange.
				Attachment file = Attachment.Create(new Attachment
				{
					Content = stream,
					FileName = "Files/Koala.jpg",
					ContentType = "image/jpg"
				});
				long contentLength = stream.Length;

				// Act.
				Attachment myFile = Attachment.Find(file.ID);
				long myContentLength = myFile.Content.Length;

				// Assert.
				Assert.That(myContentLength, Is.EqualTo(contentLength));
			}
		}

		[Test]
		public void AssetAttachmentIsNotLoadedIfNotUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);

			// Assert.
			Assert.That(theAsset.File.IsLoaded, Is.False);
		}

		[Test]
		public void AssetAttachmentIsLoadedIfFileNameIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);
			string fileName = theAsset.File.FileName;

			// Assert.
			Assert.That(fileName, Is.EqualTo("Koala.jpg"));
			Assert.That(theAsset.File.IsLoaded, Is.True);
			Assert.That(theAsset.File.ContentType, Is.EqualTo("image/jpg"));
			Assert.That(theAsset.File.Content, Is.Not.Null);
		}

		[Test]
		public void AssetAttachmentIsLoadedIfContentTypeIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);
			string contentType = theAsset.File.ContentType;

			// Assert.
			Assert.That(contentType, Is.EqualTo("image/jpg"));
			Assert.That(theAsset.File.IsLoaded, Is.True);
			Assert.That(theAsset.File.FileName, Is.EqualTo("Koala.jpg"));
			Assert.That(theAsset.File.Content, Is.Not.Null);
		}

		[Test]
		public void AssetAttachmentContentIsNotLoadedIfContentTypeIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);
			string contentType = theAsset.File.ContentType;

			// Assert.
			Assert.That(theAsset.File.IsContentLoaded, Is.False);
		}

		[Test]
		public void AssetAttachmentIsLoadedIfContentIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);
			Stream content = theAsset.File.Content;

			// Assert.
			Assert.That(content, Is.Not.Null);
			Assert.That(theAsset.File.IsLoaded, Is.True);
			Assert.That(theAsset.File.FileName, Is.EqualTo("Koala.jpg"));
			Assert.That(theAsset.File.ContentType, Is.EqualTo("image/jpg"));
		}

		[Test]
		public void AssetAttachmentContentIsLoadedIfContentIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			var asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.Find(asset.ID);
			Stream content = theAsset.File.Content;

			// Assert.
			Assert.That(theAsset.File.IsContentLoaded, Is.True);
		}
	}
}