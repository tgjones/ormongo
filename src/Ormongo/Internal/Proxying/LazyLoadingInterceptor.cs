using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace Ormongo.Internal.Proxying
{
	internal class LazyLoadingInterceptor : IInterceptor
	{
		private readonly List<string> _loadedProperties = new List<string>();

		public void Intercept(IInvocation invocation)
		{
			var document = invocation.InvocationTarget as IDocument;
			if (document != null)
			{
				// Intercept calls to ReferencesOn relations.
				if (invocation.Method.Name.StartsWith("get_") &&
					ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), invocation.Method.ReturnType))
				{
					var propertyName = invocation.Method.Name.Substring(4);
					if (!_loadedProperties.Contains(propertyName))
					{
						if (document.ReferencesOneIDs.ContainsKey(propertyName))
						{
							var otherID = document.ReferencesOneIDs[propertyName];

							var otherType = invocation.Method.ReturnType;
							var other = OrmongoConfiguration.GetMongoDatabase()
								.GetCollection(CollectionUtility.GetCollectionName(otherType))
								.FindOneByIdAs(otherType, otherID);

							var propertySetter = document.GetType().GetProperty(propertyName);
							propertySetter.SetValue(document, other, null);
						}
						_loadedProperties.Add(propertyName);
					}
				}

				// Intercept calls to ReferencesMany relations.
				else if (invocation.Method.Name.StartsWith("get_") &&
					ReflectionUtility.IsListOfRawGeneric(typeof(Document<>), invocation.Method.ReturnType))
				{
					var propertyName = invocation.Method.Name.Substring(4);
					if (!_loadedProperties.Contains(propertyName))
					{
						if (document.ReferencesManyIDs.ContainsKey(propertyName))
						{
							var otherIDs = document.ReferencesManyIDs[propertyName];
							var otherIDsBson = otherIDs.Select(id => BsonValue.Create(id));

							var otherType = ReflectionUtility.GetTypeOfGenericList(invocation.Method.ReturnType);
							var other = OrmongoConfiguration.GetMongoDatabase()
								.GetCollection(CollectionUtility.GetCollectionName(otherType))
								.FindAs(otherType, Query.In("_id", otherIDsBson))
								.Cast(otherType).ToList(otherType);

							var propertySetter = document.GetType().GetProperty(propertyName);
							propertySetter.SetValue(document, other, null);
						}
						_loadedProperties.Add(propertyName);
					}
				}
			}

			invocation.Proceed();
		}
	}
}