using MongoDB.Bson;

namespace Ormongo
{
	public class DataDictionary : BsonDocument
	{
		public T SafeGet<T>(string key)
		{
			BsonValue value;
			if (TryGetValue(key, out value))
				return (T) value.RawValue;
			return default(T);
		}

		public void SafeSet<T>(string key, T value)
		{
			if (value == null)
				Remove(key);
			else
				this[key] = BsonValue.Create(value);
		}
	}
}