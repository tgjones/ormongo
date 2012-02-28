using System;
using MongoDB.Bson;

namespace Ormongo
{
	public abstract class Plugin<T> : IPlugin<T> 
		where T : Document<T>
	{
		public virtual void Delete(ObjectId id, ref Action finalAction)
		{
			
		}

		public virtual void Find(ObjectId id, ref Func<T> finalAction)
		{
			
		}

		public virtual void Load(ref T document)
		{
			
		}

		public virtual void Save(T document, ref Action finalAction)
		{
			
		}
	}
}