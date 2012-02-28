using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using MongoDB.Bson;

namespace Ormongo.IdentityMap
{
	public class IdentityMap<T> : Plugin<T>
		where T : Document<T>
	{
		private readonly Func<IDictionary> _getCurrentHttpContextItems;
		private readonly string _identityMapKey = "Ormongo_IdentityMap_" + typeof(T).Name;

		private Dictionary<ObjectId, T> CurrentIdentityMap
		{
			get
			{
				var map = _getCurrentHttpContextItems()[_identityMapKey] as Dictionary<ObjectId, T>;
				if (map == null)
					_getCurrentHttpContextItems()[_identityMapKey] = map = new Dictionary<ObjectId, T>();
				return map;
			}
		}

		public IdentityMap(Func<IDictionary> getCurrentHttpContextItems)
		{
			_getCurrentHttpContextItems = getCurrentHttpContextItems;
		}

		public IdentityMap()
		{
			_getCurrentHttpContextItems = () => HttpContext.Current.Items;
		}

		public override void Delete(ObjectId id, ref Action finalAction)
		{
			CurrentIdentityMap.Remove(id);
		}

		public override void Find(ObjectId id, ref Func<T> finalAction)
		{
			var existingInstance = Find(id);
			if (existingInstance != null)
				finalAction = () => existingInstance;
		}

		public override void Load(ref T document)
		{
			var existingInstance = Find(document.ID);
			if (existingInstance == null)
				CurrentIdentityMap[document.ID] = document;
			else
				document = existingInstance;
		}

		public override void Save(T document, ref Action finalAction)
		{
			var previousFinalAction = finalAction;
			finalAction = () =>
			{
				previousFinalAction();
				CurrentIdentityMap[document.ID] = document;
			};
		}

		private T Find(ObjectId id)
		{
			T document;
			CurrentIdentityMap.TryGetValue(id, out document);
			return document;
		}
	}
}