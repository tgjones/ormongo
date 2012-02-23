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
			return Attachment.Create("Files/Koala.jpg", "image/jpg");
		}

		[Test]
		public void CanDeleteAttachment()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(1));

			// Act.
			Attachment.Delete(file.ID);

			// Assert.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CanCreateAttachmentFromStream()
		{
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(0));

			// Act.
			Attachment file;
			using (FileStream stream = File.OpenRead("Files/Koala.jpg"))
				file = Attachment.Create(stream, "Files/Koala.jpg", "image/jpg");

			// Assert.
			Assert.That(file.ID, Is.Not.EqualTo(ObjectId.Empty));
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(1));
		}

		[Test]
		public void CanSaveAttachment()
		{
			// Act.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(0));
			Attachment file = CreateAttachment();

			// Assert.
			Assert.That(file.ID, Is.Not.EqualTo(ObjectId.Empty));
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(1));
		}

		[Test]
		public void CanSaveExistingAttachment()
		{
			// Arrange.
			using (FileStream stream = File.OpenRead("Files/Koala.jpg"))
			{
				Attachment file = Attachment.Create(stream, "Files/Koala.jpg", "image/jpg");
				ObjectId id = file.ID;

				// Act.
				file.FileName = "newfilename.jpg";
				file.Save();

				// Assert.
				Assert.That(file.ID, Is.EqualTo(id));
				Assert.That(Attachment.FindAll().Count(), Is.EqualTo(1));
			}
		}

		[Test]
		public void CanSaveAttachmentsWithDuplicateFileNames()
		{
			// Act.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(0));
			Attachment file1 = CreateAttachment();
			Attachment file2 = CreateAttachment();

			// Assert.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(2));
			Assert.That(Attachment.FindOneByID(file1.ID), Is.Not.Null);
			Assert.That(Attachment.FindOneByID(file2.ID), Is.Not.Null);
		}

		[Test]
		public void FindByIDReturnsNullForInvalidID()
		{
			// Assert.
			Assert.That(Attachment.FindOneByID(ObjectId.GenerateNewId()), Is.Null);
		}

		[Test]
		public void CanSaveAsset()
		{
			// Arrange.
			Attachment file = CreateAttachment();

			// Act.
			Asset asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Assert.
			BsonDocument assetDoc = Asset.GetCollection().FindOneByIdAs<BsonDocument>(asset.ID);
			Assert.That(assetDoc["File"].AsObjectId, Is.EqualTo(file.ID));
		}

		[Test]
		public void CanLoadAndReadAsset()
		{
			using (FileStream stream = File.OpenRead("Files/Koala.jpg"))
			{
				// Arrange.
				Attachment file = Attachment.Create(stream, "Files/Koala.jpg", "image/jpg");
				long contentLength = stream.Length;

				// Act.
				Attachment myFile = Attachment.FindOneByID(file.ID);
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
			Asset asset = new Asset { Title = "The Title", File = file };
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
			Asset asset = new Asset { Title = "The Title", File = file };
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
			Asset asset = new Asset { Title = "The Title", File = file };
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
		public void AssetAttachmentIsLoadedIfContentIsUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			Asset asset = new Asset { Title = "The Title", File = file };
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
	}
}