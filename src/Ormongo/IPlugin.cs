using System;
using MongoDB.Bson;

namespace Ormongo
{
	public interface IPlugin<T>
		where T : Document<T>
	{
		void Delete(ObjectId id, ref Action finalAction);
		void Find(ObjectId id, ref Func<T> finalAction);
		void Load(ref T document);
		void Save(T document, ref Action finalAction);
	}
}