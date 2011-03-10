using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;
using Ormongo.Tests.DataClasses;

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
		public void CanSaveAttachmentsWithDuplicateFileNames()
		{
			// Act.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(0));
			Attachment file1 = CreateAttachment();
			Attachment file2 = CreateAttachment();

			// Assert.
			Assert.That(Attachment.FindAll().Count(), Is.EqualTo(2));
			Assert.That(Attachment.FindByID(file1.ID), Is.Not.Null);
			Assert.That(Attachment.FindByID(file2.ID), Is.Not.Null);
		}

		[Test]
		public void FindByIDReturnsNullForInvalidID()
		{
			// Assert.
			Assert.That(Attachment.FindByID(ObjectId.GenerateNewId()), Is.Null);
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
		public void AssetAttachmentIsNotLoadedIfNotUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			Asset asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.FindByID(asset.ID);

			// Assert.
			Assert.That(theAsset.File.IsLoaded, Is.False);
		}

		[Test]
		public void AssetAttachmentIsLoadedIfUsed()
		{
			// Arrange.
			Attachment file = CreateAttachment();
			Asset asset = new Asset { Title = "The Title", File = file };
			asset.Save();

			// Act.
			Asset theAsset = Asset.FindByID(asset.ID);
			string fileName = theAsset.File.FileName;

			// Assert.
			Assert.That(fileName, Is.EqualTo("Koala.jpg"));
			Assert.That(theAsset.File.IsLoaded, Is.True);
			Assert.That(theAsset.File.ContentType, Is.EqualTo("image/jpg"));
			Assert.That(theAsset.File.Content, Is.Not.Null);
		}
	}
}