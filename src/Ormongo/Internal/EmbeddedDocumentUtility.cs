using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ormongo.Internal
{
	public static class EmbeddedDocumentUtility
	{
		public static IEnumerable<object> GetEmbeddedCollection(object document, string propertyName)
		{
			// TODO: Cache this.
			var propertyInfo = document.GetType().GetProperty(propertyName);
			return GetEmbeddedCollection(document, propertyInfo);
		}

		/// <summary>
		/// Gets a list of all embedded documents in the specified document.
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		public static IEnumerable<object> GetEmbeddedDocuments(object document)
		{
			// TODO: Cache this.
			var properties = document.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			// Direct properties.
			var embeddedDocuments = properties
				.Where(pi => ReflectionUtility.IsSubclassOfRawGeneric(typeof(EmbeddedDocument<,>), pi.PropertyType))
				.Select(pi => pi.GetValue(document, null));

			// Embedded documents inside lists.
			embeddedDocuments = embeddedDocuments.Union(properties
				.Where(pi => ReflectionUtility.IsListOfRawGeneric(typeof(EmbeddedDocument<,>), pi.PropertyType))
				.SelectMany(pi => GetEmbeddedCollection(document, pi)))
				.Where(o => o != null);

			return embeddedDocuments;
		}

		private static IEnumerable<object> GetEmbeddedCollection(object document, PropertyInfo pi)
		{
			var list = pi.GetValue(document, null) as IList;
			return (list != null)
				? list.Cast<object>()
				: new List<object>();
		}

		/// <summary>
		/// Loop through each embedded document in this document,
		/// and set their parent links to this document.
		/// </summary>
		public static void UpdateParentReferences(object document)
		{
			var embeddedDocuments = GetEmbeddedDocuments(document);
			foreach (var embeddedDocument in embeddedDocuments)
			{
				var parentProperty = embeddedDocument.GetType().GetProperty("Parent");
				parentProperty.SetValue(embeddedDocument, document, null);
			}
		}

		public static string GetParentPropertyName<T, TEmbeddedIn>(T embeddedDocument)
			where T : EmbeddedDocument<T, TEmbeddedIn>
			where TEmbeddedIn : Document<TEmbeddedIn>
		{
			// TODO: Cache this.
			return typeof(TEmbeddedIn)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.PropertyType == typeof(T) || ReflectionUtility.IsListOfType(typeof(T), x.PropertyType))
				.Select(x => x.Name)
				.FirstOrDefault();
		}
	}
}