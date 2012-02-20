using System.Collections.Generic;
using MongoDB.Bson;

namespace Ormongo
{
	public class DataDictionary : BsonDocument
	{
		private readonly Dictionary<string, BsonValue> _oldValues = new Dictionary<string,BsonValue>();

		public T SafeGet<T>(string key)
		{
			BsonValue value;
			if (TryGetValue(key, out value))
				return (T) value.RawValue;
			return default(T);
		}

		public T SafeGetWas<T>(string key)
		{
			BsonValue value;
			if (_oldValues.TryGetValue(key, out value))
				return (T)value.RawValue;
			return SafeGet<T>(key);
		}

		public bool SafeGetWasChanged<T>(string key)
		{
			return _oldValues.ContainsKey(key);
		}

		public void SafeSet<T>(string key, T value)
		{
			if (!_oldValues.ContainsKey(key))
				if (Contains(key))
					_oldValues[key] = this[key];
				else
					_oldValues[key] = null;

			if (value == null)
				Remove(key);
			else
				this[key] = BsonValue.Create(value);
		}

		public void ResetChangedValues()
		{
			_oldValues.Clear();
		}
	}
}