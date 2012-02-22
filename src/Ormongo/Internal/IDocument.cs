using System.Collections.Generic;
using MongoDB.Bson;

namespace Ormongo.Internal
{
	internal interface IDocument
	{
		Dictionary<string, ObjectId> ReferencesOneIDs { get; }
		Dictionary<string, List<ObjectId>> ReferencesManyIDs { get; }
		void AfterFind();
	}
}