using System.Collections.Generic;
using MongoDB.Bson;

namespace Ormongo.Internal
{
	internal interface IDocument
	{
		Dictionary<string, ObjectId> RelationalIDs { get; }
		void AfterFind();
	}
}